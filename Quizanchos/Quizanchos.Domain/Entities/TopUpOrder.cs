using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class TopUpOrder : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public TopUpOrderStatus Status { get; set; }
    public int CoinsToCredit { get; set; }
    public decimal AmountUSDT { get; set; }
    public string Network { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? BinanceTxId { get; set; }
}
