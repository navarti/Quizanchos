using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Configurations.Game2048;
using Quizanchos.Domain.Configurations.Quiz;
using Quizanchos.Domain.Configurations.QuizMultiplayer;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Game2048;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Entities.QuizMultiplayer;

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
    public DbSet<UserMinigameScore> UserMinigameScores { get; set; }

    #region Quiz Domain Entities
    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }
    public DbSet<QuizCardFloat> QuizCardFloats { get; set; }
    public DbSet<QuizCardInt> QuizCardInts { get; set; }

    public DbSet<QuizGameSessionState> QuizGameSessionStates { get; set; }
    public DbSet<QuizSessionCard> QuizSessionCards { get; set; }
    public DbSet<QuizSessionCardAnswer> QuizSessionCardAnswers { get; set; }
    public DbSet<QuizSessionPlayerScore> QuizSessionPlayerScores { get; set; }
    #endregion

    #region Game2048 Domain Entities
    public DbSet<Game2048SessionState> Game2048SessionStates { get; set; }
    #endregion

    #region QuizMultiplayer Domain Entities
    public DbSet<QuizMultiplayerSessionState> QuizMultiplayerSessionStates { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyQuizConfiguration();
        modelBuilder.ApplyConfiguration(new Game2048SessionStateConfiguration());
        modelBuilder.ApplyConfiguration(new QuizMultiplayerSessionStateConfiguration());
    }
}
