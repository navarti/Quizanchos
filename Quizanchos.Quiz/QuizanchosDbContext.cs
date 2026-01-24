using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Quiz.Configurations;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz;

public class QuizanchosDbContext : IdentityDbContext<ApplicationUser>
{
    public QuizanchosDbContext(DbContextOptions<QuizanchosDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }
    public DbSet<SingleGameSession> SingleGameSessions { get; set; }
    public DbSet<QuizCardFloat> QuizCardFloats { get; set; }
    public DbSet<QuizCardInt> QuizCardInts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddQuizConfiguration();
    }
}
