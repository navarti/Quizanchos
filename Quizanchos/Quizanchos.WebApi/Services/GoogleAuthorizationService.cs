using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Util;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

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
        string email = authenticateResult?.Principal?.FindFirstValue(ClaimTypes.Email)
            ?? throw HandledExceptionFactory.Create("Could not retrieve email from google response");

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
            // TODO: make normal username
            UserName = Guid.NewGuid().ToString(),
            Email = email,
        };

        IdentityResult result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        result = await _userManager.AddToRoleAsync(user, QuizRole.User);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        return user;
    }
}
