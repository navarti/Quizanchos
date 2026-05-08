using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.Caravan.Services;

namespace Quizanchos.Plugin.Caravan.Extensions;

public static class CaravanServiceExtensions
{
    public static IServiceCollection AddCaravanServices(this IServiceCollection services)
    {
        services.AddScoped<CaravanEngineFactory>();
        return services;
    }
}
