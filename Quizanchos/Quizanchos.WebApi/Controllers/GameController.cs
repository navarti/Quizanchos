using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;
using System.Security.Claims;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("create")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        CreateGameResponse response = await _gameService.CreateGameAsync(request);
        return Ok(response);
    }

    [HttpPost("move")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> MakeMove([FromBody] GameRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        GameMoveResult result = await _gameService.MakeMoveAsync(request, userId);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.ErrorMessage });
        }

        return Ok(result.Response);
    }

    [HttpGet("{gameId}/state")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> GetGameState(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        GameStateResult result = await _gameService.GetGameStateAsync(gameId, minigameType);
        
        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.ErrorMessage });
        }

        return Ok(result.Response);
    }

    [HttpGet("my-active")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> GetMyActiveGame()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        ActiveGameResult result = await _gameService.GetActiveGameByPlayerIdAsync(userId);
        
        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.ErrorMessage });
        }

        return Ok(result.Response);
    }

    [HttpPost("{gameId}/finish")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> FinishGame(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        GameStateResult result = await _gameService.FinishGameAsync(gameId, minigameType);
        
        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.ErrorMessage });
        }

        return Ok(result.Response);
    }
}
