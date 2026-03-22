using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations.Market;

public class UserOwnedItemConfiguration : IEntityTypeConfiguration<UserOwnedItem>
{
    public void Configure(EntityTypeBuilder<UserOwnedItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.MarketItemId)
            .IsRequired();

        builder.Property(x => x.PurchasedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.OwnedItems)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MarketItem)
            .WithMany(x => x.OwnedByUsers)
            .HasForeignKey(x => x.MarketItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ApplicationUserId, x.MarketItemId })
            .IsUnique();
    }
}
