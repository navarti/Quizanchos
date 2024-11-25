using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[AllowAnonymous]
[Route("[controller]/[action]")]
public class AuthorizationController : Controller
{
    private readonly QuizAuthorizationService _authorizationService;

    public AuthorizationController(QuizAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<IActionResult> SignIn([FromBody] LoginModelDto loginModelDto)
    {
        await _authorizationService.SignIn(loginModelDto);
        return Ok(new { redirectUrl = "/" });
    }

    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] RegisterModelDto registerModelDto)
    {
        await _authorizationService.RegisterUser(registerModelDto);
        return Ok(new { redirectUrl = "/" });
    }

    [HttpPost]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModelDto registerModelDto)
    {
        await _authorizationService.RegisterAdmin(registerModelDto);
        return Ok();
    }
}
