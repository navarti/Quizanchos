using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.HelperServices;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class LeaderBoardService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserRetrieverService _userRetrieverService;

    public LeaderBoardService(UserManager<ApplicationUser> userManager, UserRetrieverService userRetrieverService)
    {
        _userManager = userManager;
        _userRetrieverService = userRetrieverService;
    }

    public async Task<ApplicationUserInLeaderBoardListDto> GetLeaderBoardAsync(int take, int skip)
    {
        List<ApplicationUser> users = await _userManager.Users
            .OrderByDescending(u => u.Score)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return new ApplicationUserInLeaderBoardListDto(
            users.Select((user, position) => new ApplicationUserInLeaderBoardDto(user.UserName, user.AvatarUrl, user.Score, position)).ToList());
    }

    public async Task<ApplicationUserInLeaderBoardListDto> GetUserPositionAsync(ClaimsPrincipal claimsPrincipal)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal);
        List<ApplicationUser> users = await _userManager.Users
            .OrderByDescending(u => u.Score)
            .ToListAsync();

        int position = users.FindIndex(u => u.Id == user.Id);

        return new ApplicationUserInLeaderBoardListDto(
            users.Select((user, position) => new ApplicationUserInLeaderBoardDto(user.UserName, user.AvatarUrl, user.Score, position)).ToList());
    }
}
