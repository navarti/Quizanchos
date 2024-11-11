using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Services.Realizations;
using Quizanchos.WebApi.Util;
using System.Text;

namespace Quizanchos.WebApi;

public static class Startup
{
    public static void Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionMiddlewareExtension>();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRouting();

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

        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidAudience = "A",
            ValidIssuer = "A",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = tokenValidationParameters;
        });

        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //})
        //.AddJwtBearer(options =>
        //{
        //    options.SaveToken = true;
        //    options.RequireHttpsMetadata = false;
        //    options.Audience = "A";
        //    options.Authority = "A";
        //    options.TokenValidationParameters = new TokenValidationParameters()
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        //ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidAudiences = builder.Configuration.GetSection("JWT:ValidAudience").Get<string[]>(),
        //        ValidIssuers = builder.Configuration.GetSection("JWT:ValidIssuer").Get<string[]>(),
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        //    };
        //});

        builder.Services.AddAuthorization();
        //builder.Services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        //    options.AddPolicy("User", policy => policy.RequireRole("User"));
        //}); 
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

        builder.Services.AddTransient<IJwtService, JwtService>();
        builder.Services.AddTransient<IQuizAuthorizationService, QuizAuthorizationService>(); 

        builder.Services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));

        builder.Services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        builder.Services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
        builder.Services.AddTransient<IQuizEntityService, QuizEntityService>();
        builder.Services.AddTransient<IQuizCategoryService, QuizCategoryService>();

        builder.Services.AddTransient<IClassicalQuizService, ClassicalQuizService>();
    }
}
