using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto.Market;
using Quizanchos.WebApi.Services.Market;
using System.Security.Claims;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AppRole.User)]
public class MarketController : ControllerBase
{
    private readonly MarketService _marketService;

    public MarketController(MarketService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] string type)
    {
        string userId = GetCurrentUserId();
        MarketItemType itemType = ParseType(type);

        IReadOnlyList<MarketCatalogItemDto> catalog = await _marketService.GetCatalog(itemType).ConfigureAwait(false);
        IReadOnlyList<UserOwnedItemDto> ownership = await _marketService.GetUserOwnership(userId, itemType).ConfigureAwait(false);

        HashSet<Guid> ownedIds = ownership.Select(x => x.ItemId).ToHashSet();

        var response = catalog.Select(x =>
        {
            bool isOwned = ownedIds.Contains(x.Id);
            bool isLocked = !x.IsFree && !isOwned;

            return new MarketCatalogItemStateDto(
                x.Id,
                x.Type,
                x.Name,
                x.ImageUrl,
                x.PriceCoins,
                x.IsFree,
                x.DurationMonths,
                isOwned,
                isLocked);
        }).ToList();

        return Ok(response);
    }

    [HttpGet("ownership")]
    public async Task<IActionResult> GetOwnership([FromQuery] string type)
    {
        string userId = GetCurrentUserId();
        MarketItemType itemType = ParseType(type);

        IReadOnlyList<UserOwnedItemDto> ownership = await _marketService.GetUserOwnership(userId, itemType).ConfigureAwait(false);
        return Ok(ownership);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody] MarketPurchaseRequestDto request)
    {
        if (request is null)
        {
            throw HandledExceptionFactory.CreateNullException(nameof(request));
        }

        string userId = GetCurrentUserId();
        MarketPurchaseResultDto result = await _marketService.Purchase(userId, request.MarketItemId).ConfigureAwait(false);
        return Ok(result);
    }

    private string GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw HandledExceptionFactory.Create("User is not authenticated.");
        }

        return userId;
    }

    private static MarketItemType ParseType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw HandledExceptionFactory.Create("Market item type is required.");
        }

        if (!Enum.TryParse<MarketItemType>(type, true, out var itemType))
        {
            throw HandledExceptionFactory.Create($"Unsupported market item type '{type}'.");
        }

        return itemType;
    }
}
