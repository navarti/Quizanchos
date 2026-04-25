namespace Quizanchos.WebApi.Dto.TopUp;

public record AdminPendingOrderDto(
    Guid OrderId,
    string UserId,
    string UserName,
    decimal AmountUSDT,
    string Network,
    int CoinsToCredit,
    DateTime CreatedAtUtc);
