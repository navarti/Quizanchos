using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services;

public class QuizAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public QuizAuthorizationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task SignIn(LoginModelDto loginModelDto)
    {
        _ = loginModelDto ?? throw HandledExceptionFactory.CreateNullException(nameof(loginModelDto));

        ApplicationUser? user = await _userManager.FindByEmailAsync(loginModelDto.Email);
        _ = user ?? throw HandledExceptionFactory.Create("The user with this email does not exist");

        SignInResult result = await _signInManager.PasswordSignInAsync(user, loginModelDto.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create("Invalid username or password");
        }
    }

    public async Task RegisterUser(RegisterModelDto registerModelDto) => await RegisterWithRole(registerModelDto, QuizRole.User);

    public async Task RegisterAdmin(RegisterModelDto registerModelDto) => await RegisterWithRole(registerModelDto, QuizRole.Admin);

    private async Task RegisterWithRole(RegisterModelDto registerModelDto, string roleName)
    {
        _ = registerModelDto ?? throw HandledExceptionFactory.CreateNullException(nameof(registerModelDto));

        ApplicationUser? user = await _userManager.FindByEmailAsync(registerModelDto.Email);
        if (user is not null)
        {
            throw HandledExceptionFactory.Create("The user with this email exists");
        }

        user = new ApplicationUser
        {
            // TODO: make normal username
            UserName = Guid.NewGuid().ToString(),
            Email = registerModelDto.Email,
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerModelDto.Password);
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
    }
}
