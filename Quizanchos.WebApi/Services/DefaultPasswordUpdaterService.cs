using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services;

public class DefaultPasswordUpdaterService : IUserPasswordUpdaterService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DefaultPasswordUpdaterService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterUserResult> UpdatePasswordAsync(string email, string newPassword)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(email)
            ?? throw HandledExceptionFactory.Create("The user with this email does not exist");
        
        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        return RegisterUserResult.Registered;
    }
}
