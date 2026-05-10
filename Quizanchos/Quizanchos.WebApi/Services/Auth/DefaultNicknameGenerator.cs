using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;

namespace Quizanchos.WebApi.Services.Auth;

public class DefaultNicknameGenerator
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DefaultNicknameGenerator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GenerateAsync()
    {
        int counter = await _userManager.Users.CountAsync() + 1;
        while (await _userManager.FindByNameAsync($"user{counter}") is not null)
        {
            counter++;
        }
        return $"user{counter}";
    }
}
