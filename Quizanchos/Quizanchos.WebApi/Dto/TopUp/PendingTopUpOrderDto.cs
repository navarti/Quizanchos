namespace Quizanchos.WebApi.Dto.TopUp;

public record PendingTopUpOrderDto(
    Guid OrderId,
    decimal AmountUSDT,
    string Network,
    int CoinsToCredit,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);
