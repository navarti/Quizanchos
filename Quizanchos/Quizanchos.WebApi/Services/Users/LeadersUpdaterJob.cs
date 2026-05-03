using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;

namespace Quizanchos.WebApi.Services.Users;

public class LeadersUpdaterJob : IJob
{
    private const int OneMonthPremiumPriceCoinsFallback = 500;

    private readonly QuizanchosDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMinigameRegistry _minigameRegistry;
    private readonly ILogger<LeadersUpdaterJob> _logger;

    public LeadersUpdaterJob(
        QuizanchosDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IMinigameRegistry minigameRegistry,
        ILogger<LeadersUpdaterJob> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _minigameRegistry = minigameRegistry;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime nowUtc = DateTime.UtcNow;
        int coinsReward = await GetCoinsRewardAsync().ConfigureAwait(false);
        HashSet<string> excludedUserIds = await GetExcludedUserIdsAsync().ConfigureAwait(false);

        foreach (IMinigameDescriptor descriptor in _minigameRegistry.GetAllDescriptors().Values)
        {
            ApplicationUser? winner = await FindCategoryWinnerAsync(descriptor.MinigameTypeId, excludedUserIds).ConfigureAwait(false);
            if (winner is null)
            {
                continue;
            }

            await AwardPrizeAsync(winner, descriptor, coinsReward, nowUtc).ConfigureAwait(false);
        }

        await RefreshUserStatusesAsync(nowUtc).ConfigureAwait(false);
    }

    private async Task<HashSet<string>> GetExcludedUserIdsAsync()
    {
        IList<ApplicationUser> owners = await _userManager.GetUsersInRoleAsync(AppRole.Owner).ConfigureAwait(false);
        return owners.Select(o => o.Id).ToHashSet(StringComparer.Ordinal);
    }

    private async Task<int> GetCoinsRewardAsync()
    {
        MarketItem? oneMonthPremium = await _dbContext.MarketItems
            .AsNoTracking()
            .Where(x => x.Type == MarketItemType.PremiumSubscription && x.DurationMonths == 1 && x.IsActive)
            .OrderBy(x => x.PriceCoins)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return oneMonthPremium?.PriceCoins ?? OneMonthPremiumPriceCoinsFallback;
    }

    private async Task<ApplicationUser?> FindCategoryWinnerAsync(int minigameType, HashSet<string> excludedUserIds)
    {
        string? winnerId = await _dbContext.UserMinigameScores
            .AsNoTracking()
            .Where(s => s.MinigameType == minigameType && s.Score > 0 && !excludedUserIds.Contains(s.ApplicationUserId))
            .OrderByDescending(s => s.Score)
            .Select(s => s.ApplicationUserId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return winnerId is null ? null : await _userManager.FindByIdAsync(winnerId).ConfigureAwait(false);
    }

    private async Task AwardPrizeAsync(ApplicationUser winner, IMinigameDescriptor descriptor, int coinsReward, DateTime nowUtc)
    {
        bool hasActivePremium = winner.PremiumUntilUtc.HasValue && winner.PremiumUntilUtc.Value > nowUtc;

        if (hasActivePremium)
        {
            winner.Coins += coinsReward;
            _logger.LogInformation(
                "Leaderboard prize: granted {Coins} coins to user {UserId} for being #1 in minigame {GameKey} (already premium).",
                coinsReward, winner.Id, descriptor.GameKey);
        }
        else
        {
            winner.PremiumUntilUtc = nowUtc.AddMonths(1);
            winner.Status = UserStatusEnum.Premium;
            _logger.LogInformation(
                "Leaderboard prize: granted 1-month premium to user {UserId} for being #1 in minigame {GameKey}.",
                winner.Id, descriptor.GameKey);
        }

        await _userManager.UpdateAsync(winner).ConfigureAwait(false);
    }

    private async Task RefreshUserStatusesAsync(DateTime nowUtc)
    {
        List<ApplicationUser> users = await _userManager.Users.ToListAsync().ConfigureAwait(false);

        foreach (ApplicationUser user in users)
        {
            bool hasPremium = user.PremiumUntilUtc.HasValue && user.PremiumUntilUtc.Value > nowUtc;
            UserStatusEnum desiredStatus = hasPremium ? UserStatusEnum.Premium : UserStatusEnum.Ordinary;

            if (user.Status == desiredStatus)
            {
                continue;
            }

            user.Status = desiredStatus;
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
        }
    }
}
