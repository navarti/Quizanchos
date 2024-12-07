using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class AdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUserDto>> GetUsersAsync(string name, int take, int skip)
    {
        if (take <= 0 || skip < 0)
        {
            throw HandledExceptionFactory.Create("The take must be > 0 and skip must be >= 0");
        }

        IQueryable<ApplicationUser> users = _userManager.Users;

        if (!name.IsNullOrEmpty())
        {
            users = users.Where(u => u.UserName.StartsWith(name));
        }

        return (await users.Skip(skip).Take(take).ToListAsync()).Select(u => new ApplicationUserDto(u.UserName, u.AvatarUrl, u.Score));
    }

    public async Task DeleteUser(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        _ = user ?? throw HandledExceptionFactory.Create("The user with this id does not exist");
        await _userManager.DeleteAsync(user);
    }
}
