using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.ClickCounter.Services;

namespace Quizanchos.Plugin.ClickCounter.Extensions;

public static class ClickCounterServiceExtensions
{
    public static IServiceCollection AddClickCounterServices(this IServiceCollection services)
    {
        services.AddScoped<ClickCounterEngineFactory>();
        return services;
    }
}
