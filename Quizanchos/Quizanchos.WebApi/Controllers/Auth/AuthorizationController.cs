using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Auth;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Controllers.Auth;

[Route("[controller]/[action]")]
public class AuthorizationController : Controller
{
    private readonly AuthorizationService _authorizationService;

    public AuthorizationController(AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SignIn([FromBody] LoginModelDto loginModelDto)
    {
        await _authorizationService.SignIn(loginModelDto);
        return Ok();
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SignUp([FromBody] RegisterModelDto registerModelDto)
    {
        RegisterUserResult result = await _authorizationService.RegisterUser(registerModelDto);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(AppRole.Admin)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModelDto registerModelDto)
    {
        await _authorizationService.RegisterAdmin(registerModelDto);
        return Ok();
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
    {
        await _authorizationService.RequestPasswordReset(dto);
        return Ok();
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDto dto)
    {
        await _authorizationService.ConfirmPasswordReset(dto);
        return Ok();
    }

    [HttpPost]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _authorizationService.ChangePassword(User, dto);
        return Ok();
    }
}
