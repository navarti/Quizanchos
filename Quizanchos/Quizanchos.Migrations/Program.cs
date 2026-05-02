using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Quizanchos.Domain;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<QuizanchosDbContext>(options =>
        {
            options.UseNpgsql(connectionString, opt =>
            {
                opt.MigrationsHistoryTable("__EFMigrationsHistoryQuizanshos", schema: "entity_framework");
            });
        });
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

scope.ServiceProvider.GetRequiredService<QuizanchosDbContext>().Database.Migrate();
