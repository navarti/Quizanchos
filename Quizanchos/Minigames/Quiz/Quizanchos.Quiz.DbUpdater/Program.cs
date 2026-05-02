using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quizanchos.DbUpdater.SpecificUpdaters;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Updater.FeatureUpdaters;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Domain.Repositories.Quiz.Implementations;
using Quizanchos.Domain;

namespace Quizanchos.DbUpdater;

internal class Program
{
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        using(IHost host = InitHost())
        {
            while (true)
            {
                Update(host);

                TimeSpan sleepTime = TimeSpan.FromDays(15);
                Thread.Sleep(sleepTime);
            }
        }
    }

    private static IHost InitHost()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // This ensures secrets.json is loaded during development
                //if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                var connectionString = config.GetConnectionString("DefaultConnection");

                services.AddDbContext<QuizanchosDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });

                services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
                services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
                services.AddTransient<IFeatureIntRepository, FeatureIntRepository>();
                services.AddTransient<IFeatureFloatRepository, FeatureFloatRepository>();

                services.AddTransient<FeatureUpdaterFactory>();
                services.AddTransient<IDataUpdater, DataUpdater>();
            })
            .Build();

        return host;
    }

    private static void Update(IHost host)
    {
        using (IServiceScope scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            IDataUpdater dbUpdater = scope.ServiceProvider.GetRequiredService<IDataUpdater>();
            IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            CountriesUpdater countriesUpdater = new CountriesUpdater(dbUpdater);
            countriesUpdater.UpdateSafe();

            string? moviesApiKey = config["MoviesApiKey"];
            if (string.IsNullOrEmpty(moviesApiKey))
            {
                throw new InvalidOperationException("MoviesApiKey is not set in configuration");
            }
            MoviesUpdater moviesUpdater = new MoviesUpdater(dbUpdater, moviesApiKey, maxAmountOfMovies: 100000);
            moviesUpdater.UpdateSafe();

            string? currenciesApiKey = config["CurrenciesApiKey"];
            if (string.IsNullOrEmpty(currenciesApiKey))
            {
                throw new InvalidOperationException("CurrenciesApiKey is not set in configuration");
            }
            CurrenciesUpdater currenciesUpdater = new CurrenciesUpdater(dbUpdater, currenciesApiKey);
            currenciesUpdater.UpdateSafe();
        }
    }
}
