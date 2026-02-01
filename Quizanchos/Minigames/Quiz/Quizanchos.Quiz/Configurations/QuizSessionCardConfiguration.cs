using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class QuizSessionCardConfiguration : IEntityTypeConfiguration<QuizSessionCard>
{
    public void Configure(EntityTypeBuilder<QuizSessionCard> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizGameSessionStateId)
            .IsRequired();

        builder.Property(x => x.CardIndex)
            .IsRequired();

        builder.Property(x => x.CorrectOption)
            .IsRequired();

        builder.Property(x => x.CreationTime)
            .IsRequired();

        builder.Property(x => x.EntityIdsJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.EntityNamesJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.OptionValuesJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasMany(x => x.PlayerAnswers)
            .WithOne(x => x.QuizSessionCard)
            .HasForeignKey(x => x.QuizSessionCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.QuizGameSessionStateId, x.CardIndex })
            .IsUnique();
    }
}
