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
        Quizanchos.Common.Enums.MinigameType? type = null;
        if (minigameType.HasValue)
        {
            type = System.Enum.IsDefined(typeof(Quizanchos.Common.Enums.MinigameType), minigameType.Value)
                ? (Quizanchos.Common.Enums.MinigameType?)minigameType.Value
                : null;
        }

        List<ApplicationUserInLeaderBoardDto> result = await _leaderBoardService.GetLeaderBoardAsync(take, skip, type);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPositionAsync(int? minigameType)
    {
        Quizanchos.Common.Enums.MinigameType? type = null;
        if (minigameType.HasValue)
        {
            type = System.Enum.IsDefined(typeof(Quizanchos.Common.Enums.MinigameType), minigameType.Value)
                ? (Quizanchos.Common.Enums.MinigameType?)minigameType.Value
                : null;
        }

        ApplicationUserInLeaderBoardDto result = await _leaderBoardService.GetUserPositionAsync(User, type);
        return Ok(result);
    }

}
