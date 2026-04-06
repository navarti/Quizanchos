namespace Quizanchos.WebApi.Dto.Market;

public record MarketCatalogItemDto(
    Guid Id,
    int Type,
    string Name,
    string ImageUrl,
    int PriceCoins,
    bool IsFree,
    bool IsActive,
    int? DurationMonths);
