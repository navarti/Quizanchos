using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Util;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services.Realizations;

public class GoogleAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public GoogleAuthorizationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task SignIn(AuthenticateResult authenticateResult)
    {
        string? email = authenticateResult?.Principal?.FindFirstValue(ClaimTypes.Email);
        _ = email ?? throw ExceptionFactory.Create("Could not retrieve email from google response");

        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            string userName = authenticateResult?.Principal?.FindFirstValue(ClaimTypes.Name) ?? email;
            user = await CreateNewUser(email, userName);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
    }

    private async Task<ApplicationUser> CreateNewUser(string email, string userName)
    {
        ApplicationUser user = new ApplicationUser
        {
            // Todo: make normal username
            UserName = "username",
            Email = email,
        };

        await _userManager.CreateAsync(user);

        await _userManager.AddToRoleAsync(user, "User");

        return user;
    }
}
