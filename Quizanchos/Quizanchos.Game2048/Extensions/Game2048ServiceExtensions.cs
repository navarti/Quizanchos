using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Domain.Repositories.Game2048.Implementations;
using Quizanchos.Domain.Repositories.Game2048.Interfaces;

namespace Quizanchos.Game2048.Extensions;

public static class Game2048ServiceExtensions
{
    public static IServiceCollection AddGame2048Repositories(this IServiceCollection services)
    {
        services.AddScoped<IGame2048SessionRepository, Game2048SessionRepository>();
        return services;
    }

    public static IServiceCollection AddGame2048Services(this IServiceCollection services)
    {
        services.AddScoped<Services.Game2048StateService>();
        services.AddScoped<Services.Game2048EngineFactory>();
        return services;
    }
}
