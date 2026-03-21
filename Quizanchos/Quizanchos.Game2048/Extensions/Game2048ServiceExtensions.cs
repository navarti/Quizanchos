using Microsoft.Extensions.DependencyInjection;

namespace Quizanchos.Game2048.Extensions;

public static class Game2048ServiceExtensions
{
    public static IServiceCollection AddGame2048Services(this IServiceCollection services)
    {
        services.AddScoped<Services.Game2048StateService>();
        services.AddScoped<Services.Game2048EngineFactory>();
        return services;
    }
}
