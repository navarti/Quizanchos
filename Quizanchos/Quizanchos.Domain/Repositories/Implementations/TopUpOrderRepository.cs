using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class TopUpOrderRepository : EntityRepositoryBase<Guid, TopUpOrder>, ITopUpOrderRepository
{
    public TopUpOrderRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<TopUpOrder?> FindPendingByAmountAndNetworkAsync(decimal amount, string network)
    {
        return await _dbSet.FirstOrDefaultAsync(x =>
            x.Status == TopUpOrderStatus.Pending &&
            x.AmountUSDT == amount &&
            x.Network == network).ConfigureAwait(false);
    }

    public async Task<List<TopUpOrder>> GetPendingByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(x => x.ApplicationUserId == userId && x.Status == TopUpOrderStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<TopUpOrder>> GetAllPendingAsync()
    {
        return await _dbSet
            .Include(x => x.ApplicationUser)
            .Where(x => x.Status == TopUpOrderStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<TopUpOrder>> GetExpiredCandidatesAsync(DateTime cutoffUtc)
    {
        return await _dbSet
            .Where(x => x.Status == TopUpOrderStatus.Pending && x.CreatedAtUtc < cutoffUtc)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<bool> ExistsByBinanceTxIdAsync(string txId)
    {
        return await _dbSet.AnyAsync(x => x.BinanceTxId == txId).ConfigureAwait(false);
    }
}
