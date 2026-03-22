using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IMarketItemRepository : IEntityRepository<Guid, MarketItem>
{
    Task<MarketItem?> FindByTypeAndNameAsync(MarketItemType type, string name);
}
