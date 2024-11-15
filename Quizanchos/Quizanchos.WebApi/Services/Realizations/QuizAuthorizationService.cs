using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Realizations;

public class QuizAuthorizationService : IQuizAuthorizationService
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
        _ = loginModelDto.Username ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Username));
        _ = loginModelDto.Password ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Password));

        await _signInManager.PasswordSignInAsync(loginModelDto.Username, loginModelDto.Password, isPersistent: true, lockoutOnFailure: false);
    }

    public async Task RegisterUser(RegisterModelDto registerModelDto)
    {
        _ = registerModelDto.Username ?? throw ExceptionFactory.CreateNullException(nameof(registerModelDto.Username));
        _ = registerModelDto.Password ?? throw ExceptionFactory.CreateNullException(nameof(registerModelDto.Password));

        ApplicationUser? user = await _userManager.FindByNameAsync(registerModelDto.Username);
        if (user is not null)
        {
            throw ExceptionFactory.Create("The user with this username exists");
        }

        user = new ApplicationUser
        {
            UserName = registerModelDto.Username,
        };

        await _userManager.CreateAsync(user, registerModelDto.Password);
        await _userManager.AddToRoleAsync(user, "User");

        await _signInManager.SignInAsync(user, isPersistent: true);
    }
}
