using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Services.Realizations;
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
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<QuizDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/QuizAuthorization/Login");
            options.LogoutPath = "/QuizAuthorization/Logout";
            options.Cookie = new CookieBuilder
{
                Name = "QAuth",
            };
            int.TryParse(builder.Configuration["Cookie:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenValidityInMinutes);
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));
        });
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<QuizDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddTransient<IQuizAuthorizationService, QuizAuthorizationService>(); 

        builder.Services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));

        builder.Services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        builder.Services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
        builder.Services.AddTransient<IQuizEntityService, QuizEntityService>();
        builder.Services.AddTransient<IQuizCategoryService, QuizCategoryService>();

        builder.Services.AddTransient<IClassicalQuizService, ClassicalQuizService>();
    }

    public async static Task SeedData(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await DataSeeder.SeedRoles(services);
        }
    }
}
