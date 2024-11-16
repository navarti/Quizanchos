using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Login([FromBody] LoginModelDto loginModelDto)
    {
        await _authorizationService.Login(loginModelDto);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterModelDto registerModelDto)
    {
        await _authorizationService.RegisterUser(registerModelDto);
        return Ok();
    }
}
