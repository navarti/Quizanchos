using Microsoft.Extensions.DependencyInjection;
using Quizanchos.QuizMultiplayer.Services;

namespace Quizanchos.QuizMultiplayer.Extensions;

public static class QuizMultiplayerServiceExtensions
{
    public static IServiceCollection AddQuizMultiplayerServices(this IServiceCollection services)
    {
        services.AddScoped<QuizMultiplayerEngineFactory>();
        return services;
    }
}
