namespace Quizanchos.WebApi.Dto.TopUp;

public record AdminOrderHistoryDto(
    Guid OrderId,
    string UserId,
    string UserName,
    decimal AmountUSDT,
    string Network,
    int CoinsToCredit,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
