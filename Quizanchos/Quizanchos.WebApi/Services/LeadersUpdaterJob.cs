using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

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
        foreach (ApplicationUser user in users)
        {
            user.Status = UserStatusEnum.Ordinary;
            await _userManager.UpdateAsync(user); 
        }

        users = await _leaderBoardService.GetAppUsesLeaderBoardAsync(take: PremiumUsersCount, skip: 0);
        foreach (ApplicationUser user in users)
        {
            user.Status = UserStatusEnum.Premium;
            await _userManager.UpdateAsync(user);
        }
    }
}
