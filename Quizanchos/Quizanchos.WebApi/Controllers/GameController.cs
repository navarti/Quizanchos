using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly GameEngineManager _gameEngineManager;
    private readonly IGameLogicFactory _gameLogicFactory;
    private readonly ILogger<GameController> _logger;

    public GameController(
        GameEngineManager gameEngineManager, 
        IGameLogicFactory gameLogicFactory,
        ILogger<GameController> logger)
    {
        _gameEngineManager = gameEngineManager;
        _gameLogicFactory = gameLogicFactory;
        _logger = logger;
    }

    [HttpPost("create")]
    [Authorize(AppRole.User)]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        Guid gameId = Guid.NewGuid();
        
        _logger.LogInformation("Creating game: Type={Type}, PlayerIds={PlayerIds}, Parameters={Parameters}", 
            request.MinigameType, 
            string.Join(",", request.PlayerIds),
            System.Text.Json.JsonSerializer.Serialize(request.Parameters));
        
        Dictionary<string, object> parameters = request.Parameters ?? new Dictionary<string, object>();
        IGameEngine engine = _gameLogicFactory.CreateGameEngine(
            request.MinigameType, 
            gameId, 
            request.PlayerIds.ToImmutableArray(), 
            parameters);

        _gameEngineManager.RegisterEngine(gameId, engine);

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
    public IActionResult MakeMove([FromBody] GameRequest request)
    {
        IGameEngine? engine = _gameEngineManager.GetEngine(request.GameId);
        if (engine == null)
        {
            return NotFound(new { Message = "Game not found" });
        }

        MoveResult result = engine.MakeMove(request.PlayerId, request.Move);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.Reason });
        }

        IGameState state = engine.GetState();
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
    public IActionResult GetGameState(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        _logger.LogInformation("Getting game state: GameId={GameId}, Type={Type}", gameId, minigameType);
        
        IGameEngine? engine = _gameEngineManager.GetEngine(gameId);
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
    public IActionResult GetMyActiveGame()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid playerGuid))
        {
            return Unauthorized();
        }

        IGameEngine? engine = _gameEngineManager.GetEngineByPlayer(playerGuid);
        if (engine == null)
        {
            return NotFound(new { Message = "No active game found" });
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
    public IActionResult DeleteGame(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        bool removed = _gameEngineManager.RemoveEngine(gameId);
        if (!removed)
        {
            return NotFound(new { Message = "Game not found" });
        }

        return Ok(new { Message = "Game deleted successfully" });
    }
}
