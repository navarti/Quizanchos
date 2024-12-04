using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class LeaderBoardService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaderBoardService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUserListDto> GetLeaderBoardAsync(int take, int skip)
    {
        List<ApplicationUser> users = await _userManager.Users
            .OrderByDescending(u => u.Score)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return new ApplicationUserListDto(users.Select(u => new ApplicationUserDto(u.UserName, u.AvatarUrl, u.Score)).ToList());
    }
}
