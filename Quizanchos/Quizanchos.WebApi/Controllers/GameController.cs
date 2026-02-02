using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameLogicFactory _gameLogicFactory;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IGameLogicFactory gameLogicFactory,
        IGameSessionRepository gameSessionRepository,
        ILogger<GameController> logger)
    {
        _gameLogicFactory = gameLogicFactory;
        _gameSessionRepository = gameSessionRepository;
        _logger = logger;
    }

    [HttpPost("create")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        Guid gameId = Guid.NewGuid();
        
        _logger.LogInformation("Creating game: Type={Type}, PlayerIds={PlayerIds}, Parameters={Parameters}", 
            request.MinigameType, 
            string.Join(",", request.PlayerIds),
            System.Text.Json.JsonSerializer.Serialize(request.Parameters));
        
        Dictionary<string, object> parameters = request.Parameters ?? new Dictionary<string, object>();
        IGameEngine engine = await _gameLogicFactory.CreateGameEngine(
            request.MinigameType, 
            gameId, 
            request.PlayerIds.ToImmutableArray(), 
            parameters);

        IGameState state = engine.GetState();
        _logger.LogInformation("Game created successfully: GameId={GameId}, State={State}", 
            gameId, 
            System.Text.Json.JsonSerializer.Serialize(state));

        return Ok(new CreateGameResponse
        {
            GameId = gameId,
            MinigameType = request.MinigameType,
            State = state
        });
    }

    [HttpPost("move")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> MakeMove([FromBody] GameRequest request)
    {
        // Load game session from DB
        var gameSession = await _gameSessionRepository.GetByIdAsync(request.GameId);
        if (gameSession == null)
        {
            return NotFound(new { Message = "Game not found" });
        }

        // Load engine from DB state
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(gameSession.MinigameType, request.GameId);
        if (engine == null)
        {
            return NotFound(new { Message = "Game state not found" });
        }

        MoveResult result = engine.MakeMove(request.PlayerId, request.Move);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.Reason });
        }

        // Save updated state to DB
        IGameState state = engine.GetState();
        await _gameLogicFactory.SaveGameState(gameSession.MinigameType, request.GameId, state);

        return Ok(new GameMoveResponse
        {
            Success = true,
            MinigameType = state.MinigameType,
            State = state,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner
        });
    }

    [HttpGet("{gameId}/state")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> GetGameState(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        _logger.LogInformation("Getting game state: GameId={GameId}, Type={Type}", gameId, minigameType);
        
        // Load engine from DB state
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(minigameType, gameId);
        if (engine == null)
        {
            _logger.LogWarning("Game not found: GameId={GameId}", gameId);
            return NotFound(new { Message = "Game not found" });
        }

        IGameState state = engine.GetState();
        _logger.LogInformation("Game state retrieved: GameId={GameId}, State={State}", 
            gameId, 
            System.Text.Json.JsonSerializer.Serialize(state));

        return Ok(new GameStateResponse
        {
            GameId = engine.GameId,
            MinigameType = minigameType,
            Players = engine.Players,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner,
            State = state
        });
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

        var gameSession = await _gameSessionRepository.GetActiveByPlayerIdAsync(userId);
        if (gameSession == null)
        {
            return NotFound(new { Message = "No active game found" });
        }

        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(gameSession.MinigameType, gameSession.Id);
        if (engine == null)
        {
            return NotFound(new { Message = "Game state not found" });
        }

        return Ok(new
        {
            GameId = engine.GameId,
            Players = engine.Players,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner,
            State = engine.GetState()
        });
    }

    [HttpDelete("{gameId}")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> DeleteGame(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        bool removed = await _gameSessionRepository.DeleteAsync(gameId);
        if (!removed)
        {
            return NotFound(new { Message = "Game not found" });
        }

        return Ok(new { Message = "Game deleted successfully" });
    }
}
