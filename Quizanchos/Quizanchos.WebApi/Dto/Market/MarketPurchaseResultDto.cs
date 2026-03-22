namespace Quizanchos.WebApi.Dto.Market;

public record MarketPurchaseResultDto(
    Guid ItemId,
    DateTime PurchasedAtUtc,
    int RemainingCoins);
