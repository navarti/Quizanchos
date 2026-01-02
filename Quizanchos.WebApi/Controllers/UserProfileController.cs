using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class UserProfileController : Controller
{
    private readonly UserProfileService _userProfileService;

    public UserProfileController(UserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetUserInfo()
    {
        FullApplicationUserDto userDto = await _userProfileService.GetUserInfo(User);
        return Ok(userDto);
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> UpdateAvatarUrl(string avatarUrl)
    {
        await _userProfileService.UpdateAvatarUrl(User, avatarUrl);
        return Ok();
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> UpdateAvatar(IFormFile formFile)
    {
        await _userProfileService.UpdateAvatar(User, formFile);
        return Ok();
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> UpdateNickname(string nickname)
    {
        await _userProfileService.UpdateNickname(User, nickname);
        return Ok();
    }
}
