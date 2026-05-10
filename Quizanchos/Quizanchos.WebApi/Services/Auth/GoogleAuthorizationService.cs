using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Util;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services.Auth;

public class GoogleAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly DefaultNicknameGenerator _nicknameGenerator;

    public GoogleAuthorizationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, DefaultNicknameGenerator nicknameGenerator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _nicknameGenerator = nicknameGenerator;
    }

    public async Task SignIn(AuthenticateResult authenticateResult)
    {
        string email = authenticateResult?.Principal?.FindFirstValue(ClaimTypes.Email)
            ?? throw HandledExceptionFactory.Create("Could not retrieve email from google response");

        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = await CreateNewUser(email);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
    }

    private async Task<ApplicationUser> CreateNewUser(string email)
    {
        ApplicationUser user = new ApplicationUser
        {
            UserName = await _nicknameGenerator.GenerateAsync(),
            Email = email,
            AvatarUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSFAMn65QIVqFZGQBV1otby9cY8r27W-ZGm_Q&s",
            Coins = 0,
            Status = UserStatusEnum.Ordinary,
        };

        IdentityResult result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        result = await _userManager.AddToRoleAsync(user, AppRole.User);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        return user;
    }
}
