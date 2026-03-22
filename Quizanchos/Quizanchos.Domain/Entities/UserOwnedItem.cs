using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class UserOwnedItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public Guid MarketItemId { get; set; }
    public MarketItem MarketItem { get; set; } = null!;

    public DateTime PurchasedAtUtc { get; set; }
}
