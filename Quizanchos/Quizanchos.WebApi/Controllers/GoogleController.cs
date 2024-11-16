using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class GoogleAuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GoogleAuthorizationService _googleAuthService;

    public GoogleAuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, GoogleAuthorizationService googleAuthService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _googleAuthService = googleAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> SignIn()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback))
        };
        return Challenge(authenticationProperties, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync("Google");
        if (!authenticateResult.Succeeded)
        {
            return RedirectToAction("Login", "Account");
        }

        await _googleAuthService.SignIn(authenticateResult);

        return Redirect("/");
    }
}
