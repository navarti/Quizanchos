using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IUserOwnedItemRepository : IEntityRepository<Guid, UserOwnedItem>
{
    Task<UserOwnedItem?> FindByUserAndItemAsync(string userId, Guid marketItemId);
    Task<IReadOnlyList<UserOwnedItem>> GetByUserAndTypeAsync(string userId, MarketItemType type);
}
