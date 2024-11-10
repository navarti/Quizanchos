using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Controllers;

public class AuthorizationController : Controller
{
    private readonly IAuthorizationService _authorizationService;

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModelDto loginModelDto)
    {

        return _authorizationService.Login(loginModelDto);
    }
}
