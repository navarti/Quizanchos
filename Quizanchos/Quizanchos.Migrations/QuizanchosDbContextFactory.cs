using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Quizanchos.Domain;

namespace Quizanchos.Migrations;

public class QuizanchosDbContextFactory : IDesignTimeDbContextFactory<QuizanchosDbContext>
{
    public QuizanchosDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<QuizanchosDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString, opt =>
        {
            opt.MigrationsAssembly(typeof(QuizanchosDbContextFactory).Assembly.GetName().Name);
            opt.MigrationsHistoryTable("__EFMigrationsHistory", schema: "entity_framework");
        });

        return new QuizanchosDbContext(optionsBuilder.Options);
    }
}
