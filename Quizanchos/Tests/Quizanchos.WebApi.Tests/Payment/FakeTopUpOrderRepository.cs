using System.Linq.Expressions;
using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.WebApi.Tests.Payment;

/// <summary>
/// In-memory stand-in for <see cref="ITopUpOrderRepository"/> used by the Payment unit tests.
/// Only the members exercised by <c>TopUpService</c> logic under test are implemented; the rest
/// throw, so an accidental call surfaces immediately instead of silently passing.
/// </summary>
internal sealed class FakeTopUpOrderRepository : ITopUpOrderRepository
{
    public List<TopUpOrder> Created { get; } = new();

    /// <summary>Amounts the repository should report as already taken (collision simulation).</summary>
    public HashSet<decimal> ExistingPendingAmounts { get; } = new();

    public Task<TopUpOrder> Create(TopUpOrder entity)
    {
        Created.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<TopUpOrder?> FindPendingByAmountAndNetworkAsync(decimal amount, string network)
    {
        TopUpOrder? match = ExistingPendingAmounts.Contains(amount)
            ? new TopUpOrder { AmountUSDT = amount, Network = network, Status = TopUpOrderStatus.Pending }
            : null;
        return Task.FromResult(match);
    }

    public Task<List<TopUpOrder>> GetPendingByUserIdAsync(string userId) => throw new NotSupportedException();
    public Task<List<TopUpOrder>> GetAllPendingAsync() => throw new NotSupportedException();
    public Task<List<TopUpOrder>> GetExpiredCandidatesAsync(DateTime cutoffUtc) => throw new NotSupportedException();
    public Task<bool> ExistsByBinanceTxIdAsync(string txId) => throw new NotSupportedException();
    public Task<List<TopUpOrder>> GetNonPendingAsync(int take) => throw new NotSupportedException();

    public IQueryable<TopUpOrder> Get(
        int skip = 0,
        int take = 0,
        Expression<Func<TopUpOrder, bool>>? whereExpression = null,
        Dictionary<Expression<Func<TopUpOrder, object>>, SortDirection>? orderBy = null,
        bool asNoTracking = false) => throw new NotSupportedException();

    public Task<TopUpOrder> GetById(Guid id) => throw new NotSupportedException();
    public Task<TopUpOrder?> FindById(Guid id) => throw new NotSupportedException();
    public Task<TopUpOrder> Update(TopUpOrder entity) => throw new NotSupportedException();
    public Task<IEnumerable<TopUpOrder>> GetByFilter(Expression<Func<TopUpOrder, bool>> whereExpression) => throw new NotSupportedException();
    public Task Delete(TopUpOrder entity) => throw new NotSupportedException();
    public Task<TopUpOrder?> FindFirstOrDefaultAsync(Expression<Func<TopUpOrder, bool>> whereExpression) => throw new NotSupportedException();
}
