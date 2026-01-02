using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class GoogleAuthController : Controller
{
    private const string AuthScheme = "Google";

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
    public IActionResult SignIn()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback))
        };
        return Challenge(authenticationProperties, AuthScheme);
    }

    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync(AuthScheme);
        if (!authenticateResult.Succeeded)
        {
            // TODO: change this
            return RedirectToAction("Login", "Account");
        }

        await _googleAuthService.SignIn(authenticateResult);
        return Redirect("/");
    }
}
