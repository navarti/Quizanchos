using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
[AllowAnonymous]
public class EmailConfirmationController : Controller
{
    private readonly EmailConfirmationUserRegistrationService _emailConfirmationUserRegistrationService;

    public EmailConfirmationController(EmailConfirmationUserRegistrationService emailConfirmationUserRegistrationService)
    {
        _emailConfirmationUserRegistrationService = emailConfirmationUserRegistrationService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ConfirmEmail(Guid id)
    {
        await _emailConfirmationUserRegistrationService.ConfirmEmail(id);
        return Redirect("/");
    }
}
