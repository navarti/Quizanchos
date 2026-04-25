using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface ITopUpOrderRepository : IEntityRepository<Guid, TopUpOrder>
{
    Task<TopUpOrder?> FindPendingByAmountAndNetworkAsync(decimal amount, string network);
    Task<List<TopUpOrder>> GetPendingByUserIdAsync(string userId);
    Task<List<TopUpOrder>> GetAllPendingAsync();
    Task<List<TopUpOrder>> GetExpiredCandidatesAsync(DateTime cutoffUtc);
    Task<bool> ExistsByBinanceTxIdAsync(string txId);
    Task<List<TopUpOrder>> GetNonPendingAsync(int take);
}
