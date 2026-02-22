using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Domain.Repositories.QuizMultiplayer.Implementations;
using Quizanchos.Domain.Repositories.QuizMultiplayer.Interfaces;

namespace Quizanchos.QuizMultiplayer.Extensions;

public static class QuizMultiplayerServiceExtensions
{
    public static IServiceCollection AddQuizMultiplayerRepositories(this IServiceCollection services)
    {
        services.AddScoped<IQuizMultiplayerSessionRepository, QuizMultiplayerSessionRepository>();
        return services;
    }

    public static IServiceCollection AddQuizMultiplayerServices(this IServiceCollection services)
    {
        services.AddScoped<Services.QuizMultiplayerStateService>();
        services.AddScoped<Services.QuizMultiplayerEngineFactory>();
        return services;
    }
}
