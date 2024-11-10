using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Controllers;

[AllowAnonymous]
public class QuizAuthorizationController : Controller
{
    private readonly IQuizAuthorizationService _authorizationService;

    public QuizAuthorizationController(IQuizAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModelDto loginModelDto)
    {
        TokenDto token = await _authorizationService.Login(loginModelDto);
        return  Ok(token);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterModelDto registerModelDto)
    {
        TokenDto token = await _authorizationService.RegisterUser(registerModelDto);
        return Ok(token);
    }
}
