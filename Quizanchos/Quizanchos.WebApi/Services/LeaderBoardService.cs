using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.HelperServices;
using Quizanchos.WebApi.Util;
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

    public async Task<IEnumerable<ApplicationUserInLeaderBoardDto>> GetLeaderBoardAsync(int take, int skip)
    {
        SkipTakeValidator.Validate(skip, take);

        List<ApplicationUser> users = await _userManager.Users
            .OrderByDescending(u => u.Score)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return users.Select((user, position) => new ApplicationUserInLeaderBoardDto(user.UserName, user.AvatarUrl, user.Score, position + 1));
    }

    public async Task<ApplicationUserInLeaderBoardDto> GetUserPositionAsync(ClaimsPrincipal claimsPrincipal)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal);
        List<ApplicationUser> users = await _userManager.Users
            .OrderByDescending(u => u.Score)
            .ToListAsync();

        int position = users.FindIndex(u => u.Id == user.Id);

        return new ApplicationUserInLeaderBoardDto(user.UserName, user.AvatarUrl, user.Score, position + 1);
    }
}
