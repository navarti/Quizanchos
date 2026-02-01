using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class QuizGameSessionStateConfiguration : IEntityTypeConfiguration<QuizGameSessionState>
{
    public void Configure(EntityTypeBuilder<QuizGameSessionState> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GameSessionId)
            .IsRequired();

        builder.Property(x => x.QuizCategoryId)
            .IsRequired();

        builder.Property(x => x.GameLevel)
            .IsRequired();

        builder.Property(x => x.SecondsPerCard)
            .IsRequired();

        builder.Property(x => x.OptionCount)
            .IsRequired();

        builder.Property(x => x.TotalCards)
            .IsRequired();

        builder.Property(x => x.CurrentCardIndex)
            .IsRequired();

        builder.Property(x => x.CreationTime)
            .IsRequired();

        builder.HasOne(x => x.GameSession)
            .WithMany()
            .HasForeignKey(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.QuizCategory)
            .WithMany()
            .HasForeignKey(x => x.QuizCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Cards)
            .WithOne(x => x.QuizGameSessionState)
            .HasForeignKey(x => x.QuizGameSessionStateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PlayerScores)
            .WithOne(x => x.QuizGameSessionState)
            .HasForeignKey(x => x.QuizGameSessionStateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GameSessionId)
            .IsUnique();
    }
}
