using Microsoft.EntityFrameworkCore;

namespace Quizanchos.Domain.Configurations.Quiz;

internal static class ModelBuilderExtensions
{
    public static void ApplyQuizConfiguration(this ModelBuilder modelBuilder)
    {
        // Core domain configurations
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new GameSessionConfiguration());
        modelBuilder.ApplyConfiguration(new GameSessionPlayerConfiguration());

        // Quiz configurations
        modelBuilder.ApplyConfiguration(new QuizEntityConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureCommonConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureIntConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFloatConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardFloatConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardIntConfiguration());

        // Quiz game session configurations
        modelBuilder.ApplyConfiguration(new QuizGameSessionStateConfiguration());
        modelBuilder.ApplyConfiguration(new QuizSessionCardConfiguration());
        modelBuilder.ApplyConfiguration(new QuizSessionCardAnswerConfiguration());
        modelBuilder.ApplyConfiguration(new QuizSessionPlayerScoreConfiguration());
    }
}
