using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Quiz.Configurations;

public class QuizSessionPlayerScoreConfiguration : IEntityTypeConfiguration<QuizSessionPlayerScore>
{
    public void Configure(EntityTypeBuilder<QuizSessionPlayerScore> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizGameSessionStateId)
            .IsRequired();

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.Score)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.QuizGameSessionStateId, x.ApplicationUserId })
            .IsUnique();
    }
}
