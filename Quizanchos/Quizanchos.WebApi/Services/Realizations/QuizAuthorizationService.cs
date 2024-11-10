using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Realizations;

public class QuizAuthorizationService : IQuizAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public QuizAuthorizationService(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<TokenDto> Login(LoginModelDto loginModelDto)
    {
        _ = loginModelDto.Username ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Username));
        _ = loginModelDto.Password ?? throw ExceptionFactory.CreateNullException(nameof(loginModelDto.Password));
        
        ApplicationUser? user = await _userManager.FindByNameAsync(loginModelDto.Username);
        _ = user ?? throw ExceptionFactory.Create("User with this name does not exist");

        if (!await _userManager.CheckPasswordAsync(user, loginModelDto.Password))
        {
            throw ExceptionFactory.Create("Invalid password");
        }

        string accessTokenStr = await _jwtService.GenerateAcessTokenAsync(user);
        return new TokenDto
        {
            AccessToken = accessTokenStr
        };
    }

    public async Task<TokenDto> RegisterUser(RegisterModelDto registerModelDto)
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
            UserName = registerModelDto.Username
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerModelDto.Password);
        
        string accessTokenStr = await _jwtService.GenerateAcessTokenAsync(user);
        return new TokenDto
        {
            AccessToken = accessTokenStr
        };
    }
}
