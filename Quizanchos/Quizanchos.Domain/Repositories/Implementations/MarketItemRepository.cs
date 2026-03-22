using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class MarketItemRepository : EntityRepositoryBase<Guid, MarketItem>, IMarketItemRepository
{
    public MarketItemRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<MarketItem?> FindByTypeAndNameAsync(MarketItemType type, string name)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Type == type && x.Name == name).ConfigureAwait(false);
    }
}
