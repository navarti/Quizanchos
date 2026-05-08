using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Game2048.Services;

namespace Quizanchos.Game2048.Extensions;

public static class Game2048ServiceExtensions
{
    public static IServiceCollection AddGame2048Services(this IServiceCollection services)
    {
        services.AddScoped<Game2048EngineFactory>();
        return services;
    }
}
