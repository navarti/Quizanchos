using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.Domain;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Services.Realizations;
using Quizanchos.WebApi.Util;

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

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.UseRouting();

        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
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

        builder.Services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));

        builder.Services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        builder.Services.AddTransient<IQuizEntityService, QuizEntityService>();

        builder.Services.AddTransient<IClassicalQuizService, ClassicalQuizService>();
    }
}
