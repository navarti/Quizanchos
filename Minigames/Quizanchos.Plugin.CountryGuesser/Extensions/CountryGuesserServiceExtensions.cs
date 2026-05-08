using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.CountryGuesser.Data;
using Quizanchos.Plugin.CountryGuesser.Services;

namespace Quizanchos.Plugin.CountryGuesser.Extensions;

public static class CountryGuesserServiceExtensions
{
    public static IServiceCollection AddCountryGuesserServices(this IServiceCollection services)
    {
        services.AddSingleton<CountryRepository>();
        services.AddScoped<CountryGuesserEngineFactory>();
        return services;
    }
}
