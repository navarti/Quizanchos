using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Quiz.Configurations;

public class QuizSessionCardAnswerConfiguration : IEntityTypeConfiguration<QuizSessionCardAnswer>
{
    public void Configure(EntityTypeBuilder<QuizSessionCardAnswer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizSessionCardId)
            .IsRequired();

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.AnsweredAt)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.QuizSessionCardId, x.ApplicationUserId })
            .IsUnique();
    }
}
