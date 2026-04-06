using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations.Market;

public class MarketItemConfiguration : IEntityTypeConfiguration<MarketItem>
{
    public void Configure(EntityTypeBuilder<MarketItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.PriceCoins)
            .IsRequired();

        builder.Property(x => x.IsFree)
            .IsRequired();

        builder.Property(x => x.DurationMonths)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.Type, x.Name })
            .IsUnique();

        builder.HasCheckConstraint("CK_MarketItems_PriceCoins_NonNegative", "[PriceCoins] >= 0");
        builder.HasCheckConstraint(
            "CK_MarketItems_DurationMonths_ByType",
            "([Type] <> 2 AND [DurationMonths] IS NULL) OR ([Type] = 2 AND [DurationMonths] IS NOT NULL AND [DurationMonths] > 0)");
    }
}
