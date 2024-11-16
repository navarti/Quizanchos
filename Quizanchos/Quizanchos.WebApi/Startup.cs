using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services;
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
            googleOptions.ClientId = configuration["Auth:Google:ClientId"] 
                ?? throw CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientId");
            googleOptions.ClientSecret = configuration["Auth:Google:ClientSecret"] 
                ?? throw CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientSecret");
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/QuizAuthorization/Login");
            options.LogoutPath = "/QuizAuthorization/Logout";
            options.Cookie = new CookieBuilder
            {
                Name = "QAuth",
            };
            if (!int.TryParse(configuration["Auth:Cookie:TokenValidityInMinutes"], out int tokenValidityInMinutes))
            {
                throw CriticalExceptionFactory.CreateConfigException("Auth:Cookie:TokenValidityInMinutes");
            }
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenValidityInMinutes);
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(QuizPolicy.Owner, policy => policy.RequireRole(QuzRole.Owner));
            options.AddPolicy(QuizPolicy.Admin, policy => policy.RequireRole(QuzRole.Owner, QuzRole.Admin));
            options.AddPolicy(QuizPolicy.User, policy => policy.RequireRole(QuzRole.Owner, QuzRole.Admin, QuzRole.User));
        });
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.AddControllersWithViews();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        services.AddDbContext<QuizDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw CriticalExceptionFactory.CreateConfigException("DefaultConnection"));
        });

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddTransient<QuizAuthorizationService>(); 

        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));

        services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();

        services.AddTransient<QuizEntityService>();
        services.AddTransient<QuizCategoryService>();

        services.AddTransient<GoogleAuthorizationService>();
        services.AddTransient<SingleGameSessionService>();
    }

    public async static Task SeedData(this WebApplication app, ConfigurationManager configuration)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            await DataSeeder.SeedDatabase(services, configuration);
        }
    }
}
