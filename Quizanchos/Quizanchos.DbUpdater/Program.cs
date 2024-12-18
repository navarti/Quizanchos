using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Quizanchos.DbUpdater.SpecificUpdaters;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Updater.FeatureUpdaters;
using Quizanchos.Domain;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;

namespace Quizanchos.DbUpdater;

internal class Program
{
    public static void Main(string[] args)
    {
        using(IHost host = InitHost())
        {
            Update(host);
        }
    }

    private static IHost InitHost()
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

                // TODO: Review lifetime of services and choose most appropriate one
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
            if (moviesApiKey.IsNullOrEmpty())
            {
                throw new InvalidOperationException("MoviesApiKey is not set in configuration");
            }
            MoviesUpdater moviesUpdater = new MoviesUpdater(dbUpdater, moviesApiKey, maxAmountOfMovies: 100000);
            moviesUpdater.UpdateSafe();

            string? currenciesApiKey = config["CurrenciesApiKey"];
            if (currenciesApiKey.IsNullOrEmpty())
            {
                throw new InvalidOperationException("CurrenciesApiKey is not set in configuration");
            }
            CurrenciesUpdater currenciesUpdater = new CurrenciesUpdater(dbUpdater, currenciesApiKey);
            currenciesUpdater.UpdateSafe();
        }
    }
}
