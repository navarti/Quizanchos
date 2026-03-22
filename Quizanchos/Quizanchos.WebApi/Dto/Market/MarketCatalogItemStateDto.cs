namespace Quizanchos.WebApi.Dto.Market;

public record MarketCatalogItemStateDto(
    Guid Id,
    int Type,
    string Name,
    string ImageUrl,
    int PriceCoins,
    bool IsFree,
    bool IsOwned,
    bool IsLocked);
