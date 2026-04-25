using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

public class TopUpOrderConfiguration : IEntityTypeConfiguration<TopUpOrder>
{
    public void Configure(EntityTypeBuilder<TopUpOrder> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CoinsToCredit)
            .IsRequired();

        builder.Property(x => x.AmountUSDT)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(x => x.Network)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.BinanceTxId)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.AmountUSDT, x.Network, x.Status });

        builder.HasCheckConstraint("CK_TopUpOrders_AmountUSDT_Positive", "[AmountUSDT] > 0");
        builder.HasCheckConstraint("CK_TopUpOrders_CoinsToCredit_Positive", "[CoinsToCredit] > 0");
    }
}
