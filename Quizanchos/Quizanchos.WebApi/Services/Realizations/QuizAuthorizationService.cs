using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Realizations;

public class QuizAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public QuizAuthorizationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task Login(LoginModelDto loginModelDto)
    {
        _ = loginModelDto.Email ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Email));
        _ = loginModelDto.Password ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Password));

        ApplicationUser? user = await _userManager.FindByEmailAsync(loginModelDto.Email);
        _ = user ?? throw ExceptionFactory.Create("The user with this email does not exist");

        SignInResult result = await _signInManager.PasswordSignInAsync(user, loginModelDto.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw ExceptionFactory.Create("Invalid username or password");
        }
    }

    public async Task RegisterUser(RegisterModelDto registerModelDto)
    {
        _ = registerModelDto.Email ?? throw ExceptionFactory.CreateNullException(nameof(registerModelDto.Email));
        _ = registerModelDto.Password ?? throw ExceptionFactory.CreateNullException(nameof(registerModelDto.Password));

        ApplicationUser? user = await _userManager.FindByEmailAsync(registerModelDto.Email);
        if (user is not null)
        {
            throw ExceptionFactory.Create("The user with this email exists");
        }

        user = new ApplicationUser
        {
            // Todo: make normal username
            UserName = "username",
            Email = registerModelDto.Email,
        };

        await _userManager.CreateAsync(user, registerModelDto.Password);
        await _userManager.AddToRoleAsync(user, "User");

        await _signInManager.SignInAsync(user, isPersistent: true);
    }
}
