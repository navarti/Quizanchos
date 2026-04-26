using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Services.Auth;

namespace Quizanchos.WebApi.Controllers.Auth;

[Route("[controller]/[action]")]
[AllowAnonymous]
public class EmailConfirmationController : Controller
{
    private readonly EmailConfirmationUserRegistrationService _emailConfirmationUserRegistrationService;

    public EmailConfirmationController(EmailConfirmationUserRegistrationService emailConfirmationUserRegistrationService)
    {
        _emailConfirmationUserRegistrationService = emailConfirmationUserRegistrationService;
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(string code)
    {
        await _emailConfirmationUserRegistrationService.ConfirmEmail(code);
        return Ok();
    }
}
