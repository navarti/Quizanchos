using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.QuizMultiplayer;

namespace Quizanchos.Domain.Configurations.QuizMultiplayer;

public class QuizMultiplayerSessionStateConfiguration : IEntityTypeConfiguration<QuizMultiplayerSessionState>
{
    public void Configure(EntityTypeBuilder<QuizMultiplayerSessionState> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GameSessionId)
            .IsRequired();

        builder.Property(x => x.StateJson)
            .IsRequired();

        builder.Property(x => x.CreationTime)
            .IsRequired();

        builder.HasOne(x => x.GameSession)
            .WithMany()
            .HasForeignKey(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GameSessionId)
            .IsUnique();
    }
}
