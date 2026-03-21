using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.Controllers.Users;

[Route("[controller]/[action]")]
public class LeaderBoardController : Controller
{
    private readonly LeaderBoardService _leaderBoardService;

    public LeaderBoardController(LeaderBoardService leaderBoardService)
    {
        _leaderBoardService = leaderBoardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaderBoardAsync(int take, int skip, int? minigameType)
    {
        List<ApplicationUserInLeaderBoardDto> result = await _leaderBoardService.GetLeaderBoardAsync(take, skip, minigameType);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPositionAsync(int? minigameType)
    {
        ApplicationUserInLeaderBoardDto result = await _leaderBoardService.GetUserPositionAsync(User, minigameType);
        return Ok(result);
    }

}
