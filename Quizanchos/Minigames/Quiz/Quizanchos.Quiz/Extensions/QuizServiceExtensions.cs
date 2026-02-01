using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Quiz.Repositories.Interfaces;
using Quizanchos.Quiz.Repositories.Realizations;
using Quizanchos.Quiz.Services;

namespace Quizanchos.Quiz.Extensions;

public static class QuizServiceExtensions
{
    public static IServiceCollection AddQuizDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<QuizDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        }, ServiceLifetime.Scoped);

        return services;
    }

    public static IServiceCollection AddQuizRepositories(this IServiceCollection services)
    {
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepositoryBase<,>));
        services.AddTransient<IQuizEntityRepository, QuizEntityRepository>();
        services.AddTransient<IQuizCategoryRepository, QuizCategoryRepository>();
        services.AddTransient<IFeatureFloatRepository, FeatureFloatRepository>();
        services.AddTransient<IFeatureIntRepository, FeatureIntRepository>();
        services.AddTransient<ISingleGameSessionRepository, SingleGameSessionRepository>();
        services.AddTransient<IQuizCardFloatRepository, QuizCardFloatRepository>();
        services.AddTransient<IQuizCardIntRepository, QuizCardIntRepository>();

        return services;
    }

    public static IServiceCollection AddQuizServices(this IServiceCollection services)
    {
        services.AddSingleton<LockerService>();
        services.AddScoped<QuizEntityService>();
        services.AddScoped<QuizCategoryService>();
        services.AddScoped<FeatureIntService>();
        services.AddScoped<FeatureFloatService>();
        services.AddScoped<QuizCardFloatService>();
        services.AddScoped<QuizCardIntService>();
        services.AddScoped<MainQuizCardService>();
        services.AddScoped<SessionTerminatorService>();
        services.AddScoped<QuizCardGeneratorService>();
        services.AddScoped<QuizEngineFactory>();

        return services;
    }
}
