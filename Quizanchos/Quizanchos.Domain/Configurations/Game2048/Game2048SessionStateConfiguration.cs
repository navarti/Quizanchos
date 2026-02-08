using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Game2048;

namespace Quizanchos.Domain.Configurations.Game2048;

public class Game2048SessionStateConfiguration : IEntityTypeConfiguration<Game2048SessionState>
{
    public void Configure(EntityTypeBuilder<Game2048SessionState> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GameSessionId)
            .IsRequired();

        builder.Property(x => x.Size)
            .IsRequired();

        builder.Property(x => x.BoardJson)
            .IsRequired();

        builder.Property(x => x.Score)
            .IsRequired();

        builder.Property(x => x.BestTile)
            .IsRequired();

        builder.Property(x => x.MoveCount)
            .IsRequired();

        builder.Property(x => x.CreationTime)
            .IsRequired();

        builder.HasOne(x => x.GameSession)
            .WithMany()
            .HasForeignKey(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
