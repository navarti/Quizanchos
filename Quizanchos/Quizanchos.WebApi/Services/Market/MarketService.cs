using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto.Market;
using System.Linq.Expressions;

namespace Quizanchos.WebApi.Services.Market;

public class MarketService
{
    private readonly QuizanchosDbContext _dbContext;
    private readonly IMarketItemRepository _marketItemRepository;
    private readonly IUserOwnedItemRepository _userOwnedItemRepository;

    public MarketService(
        QuizanchosDbContext dbContext,
        IMarketItemRepository marketItemRepository,
        IUserOwnedItemRepository userOwnedItemRepository)
    {
        _dbContext = dbContext;
        _marketItemRepository = marketItemRepository;
        _userOwnedItemRepository = userOwnedItemRepository;
    }

    public async Task<IReadOnlyList<MarketCatalogItemDto>> GetCatalog(MarketItemType itemType)
    {
        var orderBy = new Dictionary<Expression<Func<MarketItem, object>>, SortDirection>
        {
            { x => x.IsFree, SortDirection.Descending },
            { x => x.PriceCoins, SortDirection.Ascending },
            { x => x.Name, SortDirection.Ascending }
        };

        var items = await _marketItemRepository
            .Get(whereExpression: x => x.Type == itemType && x.IsActive, orderBy: orderBy, asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        return items
            .Select(x => new MarketCatalogItemDto(
                x.Id,
                (int)x.Type,
                x.Name,
                x.ImageUrl,
                x.PriceCoins,
                x.IsFree,
                x.IsActive))
            .ToList();
    }

    public async Task<IReadOnlyList<UserOwnedItemDto>> GetUserOwnership(string userId, MarketItemType itemType)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw HandledExceptionFactory.CreateNullException(nameof(userId));
        }

        var ownedItems = await _userOwnedItemRepository
            .GetByUserAndTypeAsync(userId, itemType)
            .ConfigureAwait(false);

        return ownedItems
            .Select(x => new UserOwnedItemDto(x.MarketItemId, x.PurchasedAtUtc))
            .ToList();
    }

    public async Task<MarketPurchaseResultDto> Purchase(string userId, Guid marketItemId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw HandledExceptionFactory.CreateNullException(nameof(userId));
        }

        if (marketItemId == Guid.Empty)
        {
            throw HandledExceptionFactory.Create("Invalid market item id.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var marketItem = await _marketItemRepository.FindById(marketItemId).ConfigureAwait(false)
                ?? throw HandledExceptionFactory.CreateIdNotFoundException(marketItemId);

            if (!marketItem.IsActive)
            {
                throw HandledExceptionFactory.Create("This item is not available for purchase.");
            }

            var existingOwnership = await _userOwnedItemRepository
                .FindByUserAndItemAsync(userId, marketItemId)
                .ConfigureAwait(false);

            if (existingOwnership is not null)
            {
                throw HandledExceptionFactory.Create("Item is already owned.");
            }

            var user = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId)
                .ConfigureAwait(false)
                ?? throw HandledExceptionFactory.Create("User was not found.");

            if (!marketItem.IsFree)
            {
                var updatedRows = await _dbContext.ApplicationUsers
                    .Where(x => x.Id == userId && x.Coins >= marketItem.PriceCoins)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(x => x.Coins, x => x.Coins - marketItem.PriceCoins))
                    .ConfigureAwait(false);

                if (updatedRows == 0)
                {
                    throw HandledExceptionFactory.Create("Not enough coins.");
                }
            }

            var ownership = new UserOwnedItem
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = userId,
                MarketItemId = marketItem.Id,
                PurchasedAtUtc = DateTime.UtcNow
            };

            await _dbContext.UserOwnedItems.AddAsync(ownership).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            int remainingCoins = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.Coins)
                .FirstAsync()
                .ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);

            return new MarketPurchaseResultDto(ownership.MarketItemId, ownership.PurchasedAtUtc, remainingCoins);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw HandledExceptionFactory.Create("Could not complete purchase.");
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<MarketCatalogItemDto> GetUsableItemForUser(string userId, Guid marketItemId, MarketItemType expectedType)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw HandledExceptionFactory.CreateNullException(nameof(userId));
        }

        if (marketItemId == Guid.Empty)
        {
            throw HandledExceptionFactory.Create("Invalid market item id.");
        }

        var marketItem = await _marketItemRepository.FindById(marketItemId).ConfigureAwait(false)
            ?? throw HandledExceptionFactory.CreateIdNotFoundException(marketItemId);

        if (!marketItem.IsActive)
        {
            throw HandledExceptionFactory.Create("This item is not available.");
        }

        if (marketItem.Type != expectedType)
        {
            throw HandledExceptionFactory.Create("Item type is invalid for this action.");
        }

        if (!marketItem.IsFree)
        {
            var existingOwnership = await _userOwnedItemRepository
                .FindByUserAndItemAsync(userId, marketItemId)
                .ConfigureAwait(false);

            if (existingOwnership is null)
            {
                throw HandledExceptionFactory.Create("Item is not owned.");
            }
        }

        return new MarketCatalogItemDto(
            marketItem.Id,
            (int)marketItem.Type,
            marketItem.Name,
            marketItem.ImageUrl,
            marketItem.PriceCoins,
            marketItem.IsFree,
            marketItem.IsActive);
    }
}
