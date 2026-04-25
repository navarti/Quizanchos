namespace Quizanchos.WebApi.Dto.TopUp;

public record CreateTopUpOrderResultDto(
    Guid OrderId,
    string WalletAddress,
    decimal AmountUSDT,
    string Network,
    DateTime ExpiresAtUtc);
