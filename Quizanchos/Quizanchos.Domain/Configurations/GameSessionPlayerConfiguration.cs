using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

public class GameSessionPlayerConfiguration : IEntityTypeConfiguration<GameSessionPlayer>
{
    public void Configure(EntityTypeBuilder<GameSessionPlayer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GameSessionId)
            .IsRequired();

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.GameSessionId, x.ApplicationUserId })
            .IsUnique();
    }
}
