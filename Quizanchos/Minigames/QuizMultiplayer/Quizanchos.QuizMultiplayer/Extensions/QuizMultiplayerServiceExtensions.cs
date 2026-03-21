using Microsoft.Extensions.DependencyInjection;

namespace Quizanchos.QuizMultiplayer.Extensions;

public static class QuizMultiplayerServiceExtensions
{
    public static IServiceCollection AddQuizMultiplayerServices(this IServiceCollection services)
    {
        services.AddScoped<Services.QuizMultiplayerStateService>();
        services.AddScoped<Services.QuizMultiplayerEngineFactory>();
        return services;
    }
}
