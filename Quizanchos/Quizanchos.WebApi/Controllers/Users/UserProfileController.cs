using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.Controllers.Users;

[Route("[controller]/[action]")]
public class UserProfileController : Controller
{
    private readonly UserProfileService _userProfileService;

    public UserProfileController(UserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> GetUserInfo()
    {
        FullApplicationUserDto userDto = await _userProfileService.GetUserInfo(User);
        return Ok(userDto);
    }

    [HttpPost]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> UpdateAvatarUrl(string avatarUrl)
    {
        await _userProfileService.UpdateAvatarUrl(User, avatarUrl);
        return Ok();
    }

    [HttpPost]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> UpdateAvatar(IFormFile formFile)
    {
        await _userProfileService.UpdateAvatar(User, formFile);
        return Ok();
    }

    [HttpPost]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> UpdateNickname(string nickname)
    {
        await _userProfileService.UpdateNickname(User, nickname);
        return Ok();
    }

    [HttpPost]
    [Authorize(AppRole.Admin)]
    public async Task<IActionResult> AddCoins(int coinsToAdd)
    {
        await _userProfileService.AddCoins(User, coinsToAdd);
        return Ok();
    }

    [HttpGet]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> ExportData()
    {
        UserDataExportDto data = await _userProfileService.ExportDataAsync(User);
        Response.Headers.ContentDisposition = "attachment; filename=user-data.json";
        return Ok(data);
    }

    [HttpDelete]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
    {
        await _userProfileService.DeleteAccountAsync(User, dto.CurrentPassword);
        return Ok();
    }
}
