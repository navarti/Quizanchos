using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class GoogleAuthController : Controller
{
    [HttpGet]
    public async Task<IActionResult> SignIn()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };

        ChallengeResult challengeResult = Challenge(authenticationProperties, "Google");

        return challengeResult;
    }
}
