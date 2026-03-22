using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class UserOwnedItemRepository : EntityRepositoryBase<Guid, UserOwnedItem>, IUserOwnedItemRepository
{
    public UserOwnedItemRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserOwnedItem?> FindByUserAndItemAsync(string userId, Guid marketItemId)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.MarketItemId == marketItemId)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserOwnedItem>> GetByUserAndTypeAsync(string userId, MarketItemType type)
    {
        return await _dbSet
            .Include(x => x.MarketItem)
            .Where(x => x.ApplicationUserId == userId && x.MarketItem.Type == type)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
