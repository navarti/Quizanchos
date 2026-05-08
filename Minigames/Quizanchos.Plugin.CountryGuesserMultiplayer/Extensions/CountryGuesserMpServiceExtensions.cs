using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.CountryGuesser.Data;
using Quizanchos.Plugin.CountryGuesserMultiplayer.Services;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.Extensions;

public static class CountryGuesserMpServiceExtensions
{
    public static IServiceCollection AddCountryGuesserMpServices(this IServiceCollection services)
    {
        services.AddSingleton<CountryRepository>();
        services.AddScoped<CountryGuesserMpEngineFactory>();
        return services;
    }
}
