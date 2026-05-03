using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.ViewControllers;

[Route("Account")] // Базовый маршрут для этого контроллера
public class ProfileController : Controller
{
    private readonly UserProfileService _userProfileService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserProfileService userProfileService,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ProfileController> logger)
    {
        _userProfileService = userProfileService;
        _signInManager = signInManager;
        _logger = logger;
    }
    
    [HttpGet("Profile")] // Обработка маршрута /Account/Profile
    [Authorize(AppRole.User)]
    public async Task<IActionResult> Profile()
    {
        FullApplicationUserDto userDto = await _userProfileService.GetUserInfo(User);
        return View(userDto); 
    }

    [HttpGet("Balance")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> Balance()
    {
        FullApplicationUserDto userDto = await _userProfileService.GetUserInfo(User);
        return View(userDto);
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/");
    }
}