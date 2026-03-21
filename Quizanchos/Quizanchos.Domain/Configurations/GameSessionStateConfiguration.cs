using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

public class GameSessionStateConfiguration : IEntityTypeConfiguration<GameSessionState>
{
    public void Configure(EntityTypeBuilder<GameSessionState> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GameSessionId)
            .IsRequired();

        builder.Property(x => x.MinigameType)
            .IsRequired();

        builder.Property(x => x.StateJson)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.GameSession)
            .WithOne(x => x.State)
            .HasForeignKey<GameSessionState>(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GameSessionId)
            .IsUnique();
    }
}
