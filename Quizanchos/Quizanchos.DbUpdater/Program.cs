using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quizanchos.DbUpdater.DataSources;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Utils;
using Quizanchos.Domain;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;

namespace Quizanchos.DbUpdater;

internal class Program
{
    static IHost _host;

    public static void Main(string[] args)
    {
        InitDependencies();

        UpdateCountries();
    }

    private static void InitDependencies()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<QuizDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });

                services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
                services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
                services.AddTransient<IFeatureIntRepository, FeatureIntRepository>();
                services.AddTransient<IFeatureFloatRepository, FeatureFloatRepository>();
            })
            .Build();

        _host = host;
    }

    private static void UpdateCountries()
    {
        CountryDataSource cityDataSource = new CountryDataSource();

        List<Country> cities = cityDataSource.GetCountriesSafe();

        if (cityDataSource.Exceptions.Count > 0)
        {
            foreach (var exception in cityDataSource.Exceptions)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }

        DataToUpdate<float> dataToUpdateWithArea = DataToUpdateBuilder.BuildCountriesDataToUpdateWithArea(cities);
        DataToUpdate<int> dataToUpdateWithPopulation = DataToUpdateBuilder.BuildCountriesDataToUpdateWithPopulation(cities);

        using (IServiceScope scope = _host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            IQuizCategoryRepository quizCategoryRepo = scope.ServiceProvider.GetRequiredService<IQuizCategoryRepository>();
            IQuizEntityRepository quizEntityRepository = scope.ServiceProvider.GetRequiredService<IQuizEntityRepository>();
            IFeatureIntRepository featureIntRepository = scope.ServiceProvider.GetRequiredService<IFeatureIntRepository>();
            IFeatureFloatRepository featureFloatRepository = scope.ServiceProvider.GetRequiredService<IFeatureFloatRepository>();

            Updater.DbUpdater dbUpdater = new Updater.DbUpdater(quizCategoryRepo, quizEntityRepository, featureIntRepository, featureFloatRepository);
            dbUpdater.UpdateEntities(dataToUpdateWithArea);
            dbUpdater.UpdateEntities(dataToUpdateWithPopulation);
        }
    }
}
