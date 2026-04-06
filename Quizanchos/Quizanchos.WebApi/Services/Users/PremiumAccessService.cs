using Microsoft.EntityFrameworkCore;
using Quizanchos.Core;
using Quizanchos.Domain;

namespace Quizanchos.WebApi.Services.Users;

public class PremiumAccessService
{
    private const string PremiumRequiredMessage = "You need an active premium subscription to play this minigame.";

    private readonly QuizanchosDbContext _dbContext;
    private readonly IMinigameRegistry _minigameRegistry;

    public PremiumAccessService(QuizanchosDbContext dbContext, IMinigameRegistry minigameRegistry)
    {
        _dbContext = dbContext;
        _minigameRegistry = minigameRegistry;
    }

    public async Task EnsureUsersCanAccessMinigameAsync(IEnumerable<string> userIds, int minigameType)
    {
        var descriptor = _minigameRegistry.GetDescriptor(minigameType);
        if (descriptor?.IsPremium != true)
        {
            return;
        }

        var uniqueUserIds = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (uniqueUserIds.Count == 0)
        {
            throw new UnauthorizedAccessException(PremiumRequiredMessage);
        }

        DateTime nowUtc = DateTime.UtcNow;

        bool hasNonPremiumUser = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(x => uniqueUserIds.Contains(x.Id))
            .AnyAsync(x => !x.PremiumUntilUtc.HasValue || x.PremiumUntilUtc <= nowUtc)
            .ConfigureAwait(false);

        if (hasNonPremiumUser)
        {
            throw new UnauthorizedAccessException(PremiumRequiredMessage);
        }

        int matchedUsersCount = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .CountAsync(x => uniqueUserIds.Contains(x.Id))
            .ConfigureAwait(false);

        if (matchedUsersCount != uniqueUserIds.Count)
        {
            throw new UnauthorizedAccessException(PremiumRequiredMessage);
        }
    }
}
