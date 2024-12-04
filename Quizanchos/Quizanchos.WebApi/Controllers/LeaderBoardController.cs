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
    public async Task<ApplicationUserListDto> GetLeaderBoardAsync(int take, int skip)
    {
        return await _leaderBoardService.GetLeaderBoardAsync(take, skip);
    }
}
