using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Controllers;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services;
using Quizanchos.WebApi.Services.Interfaces;
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
        .AddEntityFrameworkStores<QuizDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication()
        .AddCookie()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = configuration.GetOption("Auth:Google:ClientId") 
                ?? throw CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientId");
            googleOptions.ClientSecret = configuration.GetOption("Auth:Google:ClientSecret") 
                ?? throw CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientSecret");
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/QuizAuthorization/Login");
            options.LogoutPath = "/Account/Logout";
            options.Cookie = new CookieBuilder
            {
                Name = "QAuth",
            };
            if (!int.TryParse(configuration.GetOption("Auth:Cookie:TokenValidityInMinutes"), out int tokenValidityInMinutes))
            {
                throw CriticalExceptionFactory.CreateConfigException("Auth:Cookie:TokenValidityInMinutes");
            }
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenValidityInMinutes);
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(QuizPolicy.Owner, policy => policy.RequireRole(QuizRole.Owner));
            options.AddPolicy(QuizPolicy.Admin, policy => policy.RequireRole(QuizRole.Owner, QuizRole.Admin));
            options.AddPolicy(QuizPolicy.User, policy => policy.RequireRole(QuizRole.Owner, QuizRole.Admin, QuizRole.User));
        });
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        services.AddDbContext<QuizDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw CriticalExceptionFactory.CreateConfigException("DefaultConnection"));
        });

        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));

        services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
        services.AddTransient<IFeatureFloatRepository, FeatureFloatRepository>();
        services.AddTransient<IFeatureIntRepository, FeatureIntRepository>();
        services.AddTransient<ISingleGameSessionRepository, SingleGameSessionRepository>();
        services.AddTransient<IQuizCardFloatRepository, QuizCardFloatRepository>();
        services.AddTransient<IQuizCardIntRepository, QuizCardIntRepository>();

        services.AddAutoMapper(typeof(MappingProfile));

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

        services.AddTransient<AdminService>();
        services.AddTransient<UserRetrieverService>();
        services.AddTransient<GoogleAuthorizationService>();
        services.AddTransient<QuizAuthorizationService>(); 
        services.AddTransient<UserProfileService>(); 
        services.AddTransient<LeaderBoardService>(); 

        services.AddSingleton<LockerService>();
        services.AddTransient<QuizEntityService>();
        services.AddTransient<QuizCategoryService>();
        services.AddTransient<FeatureIntService>();
        services.AddTransient<FeatureFloatService>();
        services.AddTransient<QuizCardFloatService>();
        services.AddTransient<QuizCardIntService>();
        services.AddTransient<MainQuizCardService>();
        services.AddTransient<SessionTerminatorService>();
        services.AddTransient<SingleGameSessionService>();

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
            });
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

        services.AddControllersWithViews();
    }
}
