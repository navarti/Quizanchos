using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Constants;
using Quizanchos.Common.Util;
using Quizanchos.WebApi.Util;
using Quizanchos.Common.Enums;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services.Auth;

public class AuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly IUserPasswordUpdaterService _userPasswordUpdaterService;

    public AuthorizationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
        IUserRegistrationService userRegistrationService, IUserPasswordUpdaterService userPasswordUpdaterService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRegistrationService = userRegistrationService;
        _userPasswordUpdaterService = userPasswordUpdaterService;
    }

    public async Task SignIn(LoginModelDto loginModelDto)
    {
        _ = loginModelDto ?? throw HandledExceptionFactory.CreateNullException(nameof(loginModelDto));

        ApplicationUser user = await _userManager.FindByEmailAsync(loginModelDto.Email) 
            ?? throw HandledExceptionFactory.Create("The user with this email does not exist");

        SignInResult result = await _signInManager.PasswordSignInAsync(user, loginModelDto.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create("Invalid username or password");
        }
    }

    public async Task RequestPasswordReset(RequestPasswordResetDto dto)
    {
        _ = dto ?? throw HandledExceptionFactory.CreateNullException(nameof(dto));
        await _userPasswordUpdaterService.RequestPasswordResetAsync(dto.Email);
    }

    public async Task ConfirmPasswordReset(ConfirmPasswordResetDto dto)
    {
        _ = dto ?? throw HandledExceptionFactory.CreateNullException(nameof(dto));
        await _userPasswordUpdaterService.ConfirmPasswordResetAsync(dto.Email, dto.Code, dto.NewPassword);
    }

    public async Task ChangePassword(ClaimsPrincipal claimsPrincipal, ChangePasswordDto dto)
    {
        _ = dto ?? throw HandledExceptionFactory.CreateNullException(nameof(dto));

        ApplicationUser user = await _userManager.GetUserAsync(claimsPrincipal)
            ?? throw HandledExceptionFactory.CreateNullException(nameof(ApplicationUser));

        IdentityResult result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create(string.Concat(result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<RegisterUserResult> RegisterUser(RegisterModelDto registerModelDto) => await RegisterWithRole(registerModelDto, AppRole.User, UserStatusEnum.Ordinary);

    public async Task<RegisterUserResult> RegisterAdmin(RegisterModelDto registerModelDto) => await RegisterWithRole(registerModelDto, AppRole.Admin, UserStatusEnum.Premium);

    private async Task<RegisterUserResult> RegisterWithRole(RegisterModelDto registerModelDto, string roleName, UserStatusEnum userStatus)
    {
        _ = registerModelDto ?? throw HandledExceptionFactory.CreateNullException(nameof(registerModelDto));

        ApplicationUser? user = await _userManager.FindByEmailAsync(registerModelDto.Email);
        if (user is not null)
        {
            throw HandledExceptionFactory.Create("The user with this email exists");
        }

        user = new ApplicationUser
        {
            Email = registerModelDto.Email,
            AvatarUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSFAMn65QIVqFZGQBV1otby9cY8r27W-ZGm_Q&s",
            Coins = 0,
            Status = userStatus,
        };

        return await _userRegistrationService.RegisterUser(user, registerModelDto.Password, roleName);
    }
}
