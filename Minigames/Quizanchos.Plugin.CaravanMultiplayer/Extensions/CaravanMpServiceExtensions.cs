using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.CaravanMultiplayer.Services;

namespace Quizanchos.Plugin.CaravanMultiplayer.Extensions;

public static class CaravanMpServiceExtensions
{
    public static IServiceCollection AddCaravanMpServices(this IServiceCollection services)
    {
        services.AddScoped<CaravanMpEngineFactory>();
        return services;
    }
}
