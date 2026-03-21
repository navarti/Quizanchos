using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Domain.Repositories.Quiz.Implementations;
using Quizanchos.Quiz.Services;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Implementations;

namespace Quizanchos.Quiz.Extensions;

public static class QuizServiceExtensions
{
    public static IServiceCollection AddQuizRepositories(this IServiceCollection services)
    {
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));
        services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
        services.AddTransient<IFeatureFloatRepository, FeatureFloatRepository>();
        services.AddTransient<IFeatureIntRepository, FeatureIntRepository>();

        return services;
    }

    public static IServiceCollection AddQuizServices(this IServiceCollection services)
    {
        services.AddSingleton<LockerService>();
        services.AddScoped<QuizEntityService>();
        services.AddScoped<QuizCategoryService>();
        services.AddScoped<FeatureIntService>();
        services.AddScoped<FeatureFloatService>();
        services.AddScoped<QuizCardGeneratorService>();
        services.AddScoped<QuizGameStateService>();
        services.AddScoped<QuizEngineFactory>();

        return services;
    }
}
