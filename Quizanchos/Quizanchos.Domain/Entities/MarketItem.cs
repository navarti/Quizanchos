using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class MarketItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public MarketItemType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int PriceCoins { get; set; }
    public bool IsFree { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserOwnedItem> OwnedByUsers { get; set; } = new List<UserOwnedItem>();
}
