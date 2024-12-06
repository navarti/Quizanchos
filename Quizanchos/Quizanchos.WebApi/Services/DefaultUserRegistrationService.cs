using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;
using Quizanchos.WebApi.Services.Other;

namespace Quizanchos.WebApi.Services;

public class DefaultUserRegistrationService : IUserRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public DefaultUserRegistrationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<RegisterUserResult> RegisterUser(ApplicationUser user, string password, string roleName)
    {
        IdentityResult result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        return RegisterUserResult.Registered;
    }
}
