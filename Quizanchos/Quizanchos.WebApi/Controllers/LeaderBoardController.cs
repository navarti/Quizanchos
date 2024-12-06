using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class LeaderBoardController : Controller
{
    private readonly LeaderBoardService _leaderBoardService;

    public LeaderBoardController(LeaderBoardService leaderBoardService)
    {
        _leaderBoardService = leaderBoardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaderBoardAsync(int take, int skip)
    {
        ApplicationUserInLeaderBoardListDto result = await _leaderBoardService.GetLeaderBoardAsync(take, skip);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPositionAsync()
    {
        ApplicationUserInLeaderBoardListDto result = await _leaderBoardService.GetUserPositionAsync(User);
        return Ok(result);
    }
}
