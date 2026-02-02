using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Controllers;
using Quizanchos.WebApi.Services.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services;

public class GameService
{
    private readonly IGameLogicFactory _gameLogicFactory;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameLogicFactory gameLogicFactory,
        IGameSessionRepository gameSessionRepository,
        ILogger<GameService> logger)
    {
        _gameLogicFactory = gameLogicFactory;
        _gameSessionRepository = gameSessionRepository;
        _logger = logger;
    }

    public async Task<CreateGameResponse> CreateGameAsync(CreateGameRequest request)
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

        return new CreateGameResponse
        {
            GameId = gameId,
            MinigameType = request.MinigameType,
            State = state
        };
    }

    public async Task<GameMoveResult> MakeMoveAsync(GameRequest request)
    {
        // Load game session from DB
        var gameSession = await _gameSessionRepository.GetByIdAsync(request.GameId);
        if (gameSession == null)
        {
            return GameMoveResult.NotFound("Game not found");
        }

        // Load engine from DB state
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(gameSession.MinigameType, request.GameId);
        if (engine == null)
        {
            return GameMoveResult.NotFound("Game state not found");
        }

        MoveResult result = engine.MakeMove(request.PlayerId, request.Move);

        if (!result.IsSuccess)
        {
            return GameMoveResult.InvalidMove(result.Reason);
        }

        // Save updated state to DB
        IGameState state = engine.GetState();
        await _gameLogicFactory.SaveGameState(gameSession.MinigameType, request.GameId, state);

        return GameMoveResult.Success(new GameMoveResponse
        {
            Success = true,
            MinigameType = state.MinigameType,
            State = state,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner
        });
    }

    public async Task<GameStateResult> GetGameStateAsync(Guid gameId, MinigameType minigameType)
    {
        _logger.LogInformation("Getting game state: GameId={GameId}, Type={Type}", gameId, minigameType);

        // Load engine from DB state
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(minigameType, gameId);
        if (engine == null)
        {
            _logger.LogWarning("Game not found: GameId={GameId}", gameId);
            return GameStateResult.NotFound("Game not found");
        }

        IGameState state = engine.GetState();
        _logger.LogInformation("Game state retrieved: GameId={GameId}, State={State}",
            gameId,
            System.Text.Json.JsonSerializer.Serialize(state));

        return GameStateResult.Success(new GameStateResponse
        {
            GameId = engine.GameId,
            MinigameType = minigameType,
            Players = engine.Players,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner,
            State = state
        });
    }

    public async Task<ActiveGameResult> GetActiveGameByPlayerIdAsync(string userId)
    {
        var gameSession = await _gameSessionRepository.GetActiveByPlayerIdAsync(userId);
        if (gameSession == null)
        {
            return ActiveGameResult.NotFound("No active game found");
        }

        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(gameSession.MinigameType, gameSession.Id);
        if (engine == null)
        {
            return ActiveGameResult.NotFound("Game state not found");
        }

        return ActiveGameResult.Success(new ActiveGameResponse
        {
            GameId = engine.GameId,
            Players = engine.Players,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner,
            State = engine.GetState()
        });
    }

    public async Task<DeleteGameResult> DeleteGameAsync(Guid gameId)
    {
        bool removed = await _gameSessionRepository.DeleteAsync(gameId);
        if (!removed)
        {
            return DeleteGameResult.NotFound("Game not found");
        }

        return DeleteGameResult.Success("Game deleted successfully");
    }
}

public record GameMoveResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public GameMoveResponse? Response { get; init; }

    public static GameMoveResult Success(GameMoveResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static GameMoveResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    public static GameMoveResult InvalidMove(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}

public record GameStateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public GameStateResponse? Response { get; init; }

    public static GameStateResult Success(GameStateResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static GameStateResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}

public record ActiveGameResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public ActiveGameResponse? Response { get; init; }

    public static ActiveGameResult Success(ActiveGameResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static ActiveGameResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}

public record ActiveGameResponse
{
    public Guid GameId { get; init; }
    public IReadOnlyList<Guid> Players { get; init; } = Array.Empty<Guid>();
    public bool IsFinished { get; init; }
    public Guid? Winner { get; init; }
    public object State { get; init; } = null!;
}

public record DeleteGameResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;

    public static DeleteGameResult Success(string message) =>
        new() { IsSuccess = true, Message = message };

    public static DeleteGameResult NotFound(string message) =>
        new() { IsSuccess = false, Message = message };
}
