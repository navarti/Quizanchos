using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Implementations;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Quiz.Extensions;
using Quizanchos.Game2048.Extensions;
using Quizanchos.Quiz.Util;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Controllers.Auth;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services;
using Quizanchos.WebApi.Services.Auth;
using Quizanchos.WebApi.Services.GameLogic;
using Quizanchos.WebApi.Services.Users;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi;

public static class Startup
{
    public static void Configure(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddlewareExtension>();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }

    public static void AddAuthorizaiton(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 1;
        })
        .AddEntityFrameworkStores<QuizanchosDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication()
        .AddCookie()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = configuration.GetOption("Auth:Google:ClientId") 
                ?? throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientId");
            googleOptions.ClientSecret = configuration.GetOption("Auth:Google:ClientSecret") 
                ?? throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientSecret");
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/Signin");
            options.AccessDeniedPath = "/Signin";
            options.Cookie = new CookieBuilder
            {
                Name = "QAuth",
            };
            if (!int.TryParse(configuration.GetOption("Auth:Cookie:TokenValidityInMinutes"), out int tokenValidityInMinutes))
            {
                throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Cookie:TokenValidityInMinutes");
            }
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenValidityInMinutes);
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppRole.Owner, policy => policy.RequireRole(AppRole.Owner));
            options.AddPolicy(AppRole.Admin, policy => policy.RequireRole(AppRole.Owner, AppRole.Admin));
            options.AddPolicy(AppRole.User, policy => policy.RequireRole(AppRole.Owner, AppRole.Admin, AppRole.User));
        });
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw Util.CriticalExceptionFactory.CreateConfigException("DefaultConnection");

        // Single unified DbContext for all entities
        services.AddDbContext<QuizanchosDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        }, ServiceLifetime.Scoped);

        services.AddQuizRepositories();
        services.AddQuizServices();

        services.AddGame2048Repositories();
        services.AddGame2048Services();

        services.AddAutoMapper(cfg => 
        { 
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<QuizMappingProfile>(); 
        });

        services.AddSingleton<ICloudinary>(serviceProvider =>
        {
            Account account = new Account(
                configuration.GetOption("Cloudinary:CloudName"),
                configuration.GetOption("Cloudinary:ApiKey"),
                configuration.GetOption("Cloudinary:ApiSecret")
            );
            return new Cloudinary(account);
        });

        services.AddSingleton<ContainerService>();

        AddControllers(builder);

        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IGameLogicFactory, GameLogicFactory>();
        services.AddScoped<GameService>();
        services.AddTransient<AdminService>();
        services.AddTransient<UserRetrieverService>();
        services.AddTransient<GoogleAuthorizationService>();
        services.AddTransient<AuthorizationService>(); 
        services.AddTransient<UserProfileService>(); 
        services.AddTransient<LeaderBoardService>(); 

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("MonthlyTask");
            q.AddJob<LeadersUpdaterJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MonthlyTaskTrigger")
                .WithCronSchedule("0 0 0 1 * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public async static Task SeedData(this WebApplication app, ConfigurationManager configuration)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            await DataSeeder.SeedDatabase(services, configuration);
        }
    }

    private static void AddControllers(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        if (configuration.GetOption("EmailConfirmation:ShouldUse") == "0")
        {
            services.AddTransient<IUserPasswordUpdaterService, DefaultPasswordUpdaterService>();
            services.AddTransient<IUserRegistrationService, DefaultUserRegistrationService>();
            services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new SkipControllerConvention(typeof(EmailConfirmationController)));
            })
            .AddJsonOptions(ConfigureJsonOptions);
            return;
        }

        services.AddFluentEmail(configuration.GetOption("EmailConfirmation:MailGun:FromEmail"), configuration.GetOption("EmailConfirmation:MailGun:FromName"))
            .AddMailGunSender(configuration.GetOption("EmailConfirmation:MailGun:Domain"), configuration.GetOption("EmailConfirmation:MailGun:ApiKey"));

        services.AddTransient<EmailSenderService>();
        services.AddTransient<DefaultUserRegistrationService>();
        services.AddTransient<EmailConfirmationUserRegistrationService>();
        services.AddTransient<EmailConfirmationPasswordUpdaterService>();
        services.AddTransient<IUserPasswordUpdaterService, EmailConfirmationPasswordUpdaterService>();
        services.AddTransient<IUserRegistrationService, EmailConfirmationUserRegistrationService>();

        services.AddControllersWithViews()
            .AddJsonOptions(ConfigureJsonOptions);
    }

    private static void ConfigureJsonOptions(Microsoft.AspNetCore.Mvc.JsonOptions options)
    {
        options.JsonSerializerOptions.TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                (typeInfo) =>
                {
                    // Configure polymorphic serialization for GameMove
                    if (typeInfo.Type == typeof(Quizanchos.Core.GameMove))
                    {
                        typeInfo.PolymorphismOptions = new System.Text.Json.Serialization.Metadata.JsonPolymorphismOptions
                        {
                            TypeDiscriminatorPropertyName = "gameType",
                            UnknownDerivedTypeHandling = System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
                            DerivedTypes =
                            {
                                new System.Text.Json.Serialization.Metadata.JsonDerivedType(typeof(Quizanchos.Quiz.GameLogic.QuizMove), "quiz"),
                                new System.Text.Json.Serialization.Metadata.JsonDerivedType(typeof(Quizanchos.Game2048.GameLogic.Game2048Move), "game2048")
                            }
                        };
                    }
                }
            }
        };
    }
}
