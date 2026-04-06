using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.ViewControllers;

[Route("Account")] // Базовый маршрут для этого контроллера
public class ProfileController : Controller
{
    private readonly UserProfileService _userProfileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(UserProfileService userProfileService, ILogger<ProfileController> logger)
    {
        _userProfileService = userProfileService;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("Identity.External");
        Response.Cookies.Delete("QAuth");
        await HttpContext.SignOutAsync("Cookies");
        return Ok();
    }
}