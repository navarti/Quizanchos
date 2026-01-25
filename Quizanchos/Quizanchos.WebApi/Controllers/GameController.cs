using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly GameEngineManager _gameEngineManager;
    private readonly GameLogicFactory _gameLogicFactory;
    private readonly ILogger<GameController> _logger;

    public GameController(
        GameEngineManager gameEngineManager, 
        GameLogicFactory gameLogicFactory,
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
        
        Dictionary<string, object> parameters = request.Parameters ?? new Dictionary<string, object>();
        object engine = _gameLogicFactory.CreateGameEngine(
            request.MinigameType, 
            gameId, 
            request.PlayerIds.ToImmutableArray(), 
            parameters);

        _gameEngineManager.RegisterEngine(gameId, request.MinigameType, (dynamic)engine);

        object state = GetEngineState(request.MinigameType, engine);

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
        MinigameType? minigameType = DetermineMinigameType(request.Move);
        if (minigameType == null)
        {
            return BadRequest(new { Message = "Unknown game type" });
        }

        return minigameType switch
        {
            MinigameType.Quiz => HandleQuizMove(request.GameId, request.PlayerId, (QuizMove)request.Move),
            _ => BadRequest(new { Message = "Unknown game type" })
        };
    }

    [HttpGet("{gameId}/state")]
    [Authorize(AppRole.User)]
    public IActionResult GetGameState(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        return minigameType switch
        {
            MinigameType.Quiz => GetQuizGameState(gameId),
            _ => BadRequest(new { Message = "Unknown game type" })
        };
    }

    [HttpDelete("{gameId}")]
    [Authorize(AppRole.User)]
    public IActionResult DeleteGame(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        bool removed = _gameEngineManager.RemoveEngine(gameId, minigameType);
        if (!removed)
        {
            return NotFound(new { Message = "Game not found" });
        }

        return Ok(new { Message = "Game deleted successfully" });
    }

    private IActionResult HandleQuizMove(Guid gameId, Guid playerId, QuizMove move)
    {
        GameEngine<QuizGameState, QuizMove>? engine = _gameEngineManager.GetEngine<QuizGameState, QuizMove>(gameId, MinigameType.Quiz);
        if (engine == null)
        {
            return NotFound(new { Message = "Quiz game not found" });
        }

        MoveResult result = engine.MakeMove(playerId, move);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.Reason });
        }

        return Ok(new GameMoveResponse
        {
            Success = true,
            MinigameType = MinigameType.Quiz,
            State = engine.State,
            IsFinished = engine.State.IsFinished,
            Winner = engine.State.Winner
        });
    }

    private IActionResult GetQuizGameState(Guid gameId)
    {
        GameEngine<QuizGameState, QuizMove>? engine = _gameEngineManager.GetEngine<QuizGameState, QuizMove>(gameId, MinigameType.Quiz);
        if (engine == null)
        {
            return NotFound(new { Message = "Quiz game not found" });
        }

        return Ok(new GameStateResponse
        {
            GameId = engine.State.GameId,
            MinigameType = MinigameType.Quiz,
            Players = engine.State.Players,
            IsFinished = engine.State.IsFinished,
            Winner = engine.State.Winner,
            State = new
            {
                CurrentCardIndex = engine.State.CurrentCardIndex,
                PlayerScores = engine.State.PlayerScores,
                TotalCards = engine.State.TotalCards
            }
        });
    }

    private MinigameType? DetermineMinigameType(GameMove move)
    {
        return move switch
        {
            QuizMove => MinigameType.Quiz,
            _ => null
        };
    }

    private object GetEngineState(MinigameType minigameType, object engine)
    {
        return minigameType switch
        {
            MinigameType.Quiz => ((GameEngine<QuizGameState, QuizMove>)engine).State,
            _ => throw new ArgumentException("Unknown engine type")
        };
    }
}
