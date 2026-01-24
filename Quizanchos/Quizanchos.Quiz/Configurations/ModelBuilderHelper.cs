using Microsoft.EntityFrameworkCore;

namespace Quizanchos.Quiz.Configurations;

public static class ModelBuilderHelper
{
    public static void AddQuizConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new QuizEntityConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureCommonConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureIntConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFloatConfiguration());
        modelBuilder.ApplyConfiguration(new SingleGameSessionConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardCommonConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardFloatConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardIntConfiguration());
    }
}
