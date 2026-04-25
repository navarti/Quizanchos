using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
    public DbSet<GameSessionState> GameSessionStates { get; set; }
    public DbSet<UserMinigameScore> UserMinigameScores { get; set; }
    public DbSet<MarketItem> MarketItems { get; set; }
    public DbSet<UserOwnedItem> UserOwnedItems { get; set; }
    public DbSet<TopUpOrder> TopUpOrders { get; set; }

    #region Quiz Domain Entities
    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }
    public DbSet<QuizCardFloat> QuizCardFloats { get; set; }
    public DbSet<QuizCardInt> QuizCardInts { get; set; }

    public DbSet<QuizSessionCard> QuizSessionCards { get; set; }
    public DbSet<QuizSessionCardAnswer> QuizSessionCardAnswers { get; set; }
    public DbSet<QuizSessionPlayerScore> QuizSessionPlayerScores { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizanchosDbContext).Assembly);
    }
}
