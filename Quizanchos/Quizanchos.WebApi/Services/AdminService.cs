using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;

namespace Quizanchos.WebApi.Services;

public class AdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task DeleteUser(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        _ = user ?? throw HandledExceptionFactory.Create("The user with this id does not exist");
        await _userManager.DeleteAsync(user);
    }
}
