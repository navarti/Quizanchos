using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain;
using Quizanchos.Quiz.Configurations;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz;

public class QuizDbContext : QuizanchosDbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }
    public DbSet<QuizCardFloat> QuizCardFloats { get; set; }
    public DbSet<QuizCardInt> QuizCardInts { get; set; }
    
    // Quiz game session entities
    public DbSet<QuizGameSessionState> QuizGameSessionStates { get; set; }
    public DbSet<QuizSessionCard> QuizSessionCards { get; set; }
    public DbSet<QuizSessionCardAnswer> QuizSessionCardAnswers { get; set; }
    public DbSet<QuizSessionPlayerScore> QuizSessionPlayerScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
