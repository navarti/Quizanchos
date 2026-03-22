namespace Quizanchos.WebApi.Dto.Market;

public record UserOwnedItemDto(
    Guid ItemId,
    DateTime PurchasedAtUtc);
