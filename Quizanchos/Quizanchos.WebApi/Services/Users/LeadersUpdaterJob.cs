using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Users;

public class LeadersUpdaterJob : IJob
{
    private const int PremiumUsersCount = 3;

    private readonly LeaderBoardService _leaderBoardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeadersUpdaterJob(LeaderBoardService leaderBoardService, UserManager<ApplicationUser> userManager)
    {
        _leaderBoardService = leaderBoardService;
        _userManager = userManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        List<ApplicationUser> users = await _userManager.Users.ToListAsync();
        HashSet<string> leaderboardPremiumUserIds = (await _leaderBoardService
            .GetAppUsesLeaderBoardAsync(take: PremiumUsersCount, skip: 0)
            .ConfigureAwait(false))
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        DateTime nowUtc = DateTime.UtcNow;

        foreach (ApplicationUser user in users)
        {
            bool hasPaidPremium = user.PremiumUntilUtc.HasValue && user.PremiumUntilUtc.Value > nowUtc;
            bool hasLeaderboardPremium = leaderboardPremiumUserIds.Contains(user.Id);

            user.Status = hasPaidPremium || hasLeaderboardPremium
                ? UserStatusEnum.Premium
                : UserStatusEnum.Ordinary;

            await _userManager.UpdateAsync(user).ConfigureAwait(false);
        }
    }
}
