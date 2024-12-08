using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
[AllowAnonymous]
public class EmailConfirmationController : Controller
{
    private readonly EmailConfirmationUserRegistrationService _emailConfirmationUserRegistrationService;
    private readonly EmailConfirmationPasswordUpdaterService _emailConfirmationPasswordUpdaterService;

    public EmailConfirmationController(EmailConfirmationUserRegistrationService emailConfirmationUserRegistrationService, EmailConfirmationPasswordUpdaterService emailConfirmationPasswordUpdaterService)
    {
        _emailConfirmationUserRegistrationService = emailConfirmationUserRegistrationService;
        _emailConfirmationPasswordUpdaterService = emailConfirmationPasswordUpdaterService;
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(string code)
    {
        await _emailConfirmationUserRegistrationService.ConfirmEmail(code);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmPassword(string code)
    {
        await _emailConfirmationPasswordUpdaterService.ConfirmEmail(code);
        return Ok();
    }
}
