using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Configurations;
using Quizanchos.Domain.Configurations.Quiz;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Domain;

public class QuizanchosDbContext : IdentityDbContext<ApplicationUser>
{
    public QuizanchosDbContext(DbContextOptions<QuizanchosDbContext> options) : base(options)
    {
    }

    // Core domain entities
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<GameSessionPlayer> GameSessionPlayers { get; set; }

    // Quiz entities
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
