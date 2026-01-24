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
        });

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
        services.AddTransient<QuizEntityService>();
        services.AddTransient<QuizCategoryService>();
        services.AddTransient<FeatureIntService>();
        services.AddTransient<FeatureFloatService>();
        services.AddTransient<QuizCardFloatService>();
        services.AddTransient<QuizCardIntService>();
        services.AddTransient<MainQuizCardService>();
        services.AddTransient<SessionTerminatorService>();

        return services;
    }
}
