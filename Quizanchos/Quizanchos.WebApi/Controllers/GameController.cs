using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
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

        _gameEngineManager.RegisterEngine(gameId, request.MinigameType, engine);

        object state = engine.GetState();
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
        MinigameType? minigameType = DetermineMinigameType(request.Move);
        if (minigameType == null)
        {
            return BadRequest(new { Message = "Unknown game type" });
        }

        IGameEngine? engine = _gameEngineManager.GetEngine(request.GameId, minigameType.Value);
        if (engine == null)
        {
            return NotFound(new { Message = "Game not found" });
        }

        MoveResult result = engine.MakeMove(request.PlayerId, request.Move);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.Reason });
        }

        // For Quiz games, generate the next card after a move
        if (minigameType == MinigameType.Quiz)
        {
            await GenerateNextQuizCardIfNeeded(request.GameId, engine);
        }

        return Ok(new GameMoveResponse
        {
            Success = true,
            MinigameType = minigameType.Value,
            State = engine.GetState(),
            IsFinished = engine.IsFinished,
            Winner = engine.Winner
        });
    }

    [HttpGet("{gameId}/state")]
    [Authorize(AppRole.User)]
    public IActionResult GetGameState(Guid gameId, [FromQuery] MinigameType minigameType)
    {
        _logger.LogInformation("Getting game state: GameId={GameId}, Type={Type}", gameId, minigameType);
        
        IGameEngine? engine = _gameEngineManager.GetEngine(gameId, minigameType);
        if (engine == null)
        {
            _logger.LogWarning("Game not found: GameId={GameId}, Type={Type}", gameId, minigameType);
            return NotFound(new { Message = "Game not found" });
        }

        object state = engine.GetState();
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
        bool removed = _gameEngineManager.RemoveEngine(gameId, minigameType);
        if (!removed)
        {
            return NotFound(new { Message = "Game not found" });
        }

        return Ok(new { Message = "Game deleted successfully" });
    }

    // TODO: Implement card generation for new game engine
    // The quiz card services (QuizCardIntService, QuizCardFloatService) are currently
    // designed to work with database-backed SingleGameSession entities.
    // They need to be refactored to work with the new in-memory GameEngine.
    // For now, the game flow is broken and needs this implementation.

    private async Task GenerateNextQuizCardIfNeeded(Guid gameId, IGameEngine engine)
    {
        _logger.LogInformation("Checking if next card needs to be generated for game {GameId}", gameId);

        GameEngineWrapper<QuizGameState, QuizMove>? wrapper = engine as GameEngineWrapper<QuizGameState, QuizMove>;
        if (wrapper == null)
        {
            _logger.LogWarning("Engine is not a Quiz game engine");
            return;
        }

        QuizGameState state = wrapper.GetTypedEngine().State;

        // Check if we need to generate the next card
        // We need a card if CurrentCardIndex points to a card that doesn't exist yet
        if (state.CurrentCardIndex >= state.Cards.Count && state.CurrentCardIndex < state.TotalCards)
        {
            _logger.LogInformation("Generating card {CardIndex} for game {GameId}", state.CurrentCardIndex, gameId);

            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var cardGenerator = scope.ServiceProvider.GetRequiredService<Quiz.Services.QuizCardGeneratorService>();
                
                try
                {
                    await cardGenerator.GenerateSingleCard(
                        state,
                        state.QuizCategoryId,
                        state.OptionCount,
                        state.GameLevel
                    );
                    
                    _logger.LogInformation("Card {CardIndex} generated successfully. Total cards: {TotalCards}", 
                        state.CurrentCardIndex, state.Cards.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating card {CardIndex} for game {GameId}", 
                        state.CurrentCardIndex, gameId);
                }
            }
        }
        else
        {
            _logger.LogInformation("No card generation needed. CurrentCardIndex={CurrentCardIndex}, Cards.Count={CardsCount}, TotalCards={TotalCards}",
                state.CurrentCardIndex, state.Cards.Count, state.TotalCards);
        }
    }

    private MinigameType? DetermineMinigameType(GameMove move)
    {
        Type moveType = move.GetType();
        
        foreach (MinigameType type in Enum.GetValues<MinigameType>())
        {
            if (_gameLogicFactory.GetMoveType(type) == moveType)
            {
                return type;
            }
        }
        
        return null;
    }
}
