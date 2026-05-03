using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Controllers;
using Quizanchos.WebApi.Models.Rooms;
using Quizanchos.WebApi.Services.GameLogic;
using Quizanchos.WebApi.Services.Users;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services;

public class GameService
{
    private readonly IGameLogicFactory _gameLogicFactory;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IGameNotifier _gameNotifier;
    private readonly ILogger<GameService> _logger;
    private readonly UserScoreService _userScoreService;
    private readonly PremiumAccessService _premiumAccessService;

    public GameService(
        IGameLogicFactory gameLogicFactory,
        IGameSessionRepository gameSessionRepository,
        IGameNotifier gameNotifier,
        ILogger<GameService> logger,
        UserScoreService userScoreService,
        PremiumAccessService premiumAccessService)
    {
        _gameLogicFactory = gameLogicFactory;
        _gameSessionRepository = gameSessionRepository;
        _gameNotifier = gameNotifier;
        _logger = logger;
        _userScoreService = userScoreService;
        _premiumAccessService = premiumAccessService;
    }

    public async Task<CreateGameResponse> CreateGameAsync(CreateGameRequest request)
    {
        await _premiumAccessService
            .EnsureUsersCanAccessMinigameAsync(request.PlayerIds, request.MinigameType)
            .ConfigureAwait(false);

        foreach (var playerId in request.PlayerIds)
        {
            var activeGame = await _gameSessionRepository.GetActiveByPlayerIdAsync(playerId);
            if (activeGame != null)
            {
                throw new InvalidOperationException($"Player {playerId} already has an active game session (GameId: {activeGame.Id})");
            }
        }

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

    public async Task<CreateGameResponse> CreateMultiPlayerGameAsync(
        int minigameType,
        IReadOnlyList<string> playerIds,
        Dictionary<string, object>? parameters,
        IReadOnlyList<TeamInfo> teams,
        IReadOnlyDictionary<string, string>? playerNicknames = null)
    {
        await _premiumAccessService
            .EnsureUsersCanAccessMinigameAsync(playerIds, minigameType)
            .ConfigureAwait(false);

        foreach (var playerId in playerIds)
        {
            var activeGame = await _gameSessionRepository.GetActiveByPlayerIdAsync(playerId);
            if (activeGame != null)
            {
                throw new InvalidOperationException(
                    $"Player {playerId} already has an active game session (GameId: {activeGame.Id})");
            }
        }

        Guid gameId = Guid.NewGuid();

        _logger.LogInformation(
            "Creating multiplayer game: Type={Type}, PlayerIds={PlayerIds}, Teams={TeamCount}",
            minigameType,
            string.Join(",", playerIds),
            teams.Count);

        Dictionary<string, object> gameParams = parameters ?? new Dictionary<string, object>();

        // Inject team composition into parameters so engine factories can use it
        if (teams.Count > 0)
        {
            var teamsData = teams.Select(t => new
            {
                teamIndex = t.TeamIndex,
                name = t.Name,
                playerIds = t.PlayerIds
            }).ToList();
            gameParams["teams"] = System.Text.Json.JsonSerializer.Serialize(teamsData);
        }

        if (playerNicknames is { Count: > 0 })
        {
            gameParams["playerNicknames"] = System.Text.Json.JsonSerializer.Serialize(playerNicknames);
        }

        IGameEngine engine = await _gameLogicFactory.CreateGameEngine(
            minigameType,
            gameId,
            playerIds.ToImmutableArray(),
            gameParams);

        IGameState state = engine.GetState();
        _logger.LogInformation("Multiplayer game created: GameId={GameId}, State={State}",
            gameId,
            System.Text.Json.JsonSerializer.Serialize(state));

        return new CreateGameResponse
        {
            GameId = gameId,
            MinigameType = minigameType,
            State = state
        };
    }

    public async Task<GameMoveResult> MakeMoveAsync(GameRequest request, string playerId)
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

        if (engine.IsFinished)
        {
            return GameMoveResult.InvalidMove("Game is already finished");
        }

        // Check if game should be finished
        var finishResult = await CheckFinish(engine, gameSession.MinigameType);
        if (finishResult != null)
        {
            return GameMoveResult.Success(new GameMoveResponse
            {
                Success = true,
                MinigameType = gameSession.MinigameType,
                State = finishResult.Response!.State,
                IsFinished = true,
                Winner = finishResult.Response.Winner
            });
        }

        MoveResult result = engine.MakeMove(playerId, request.Move);

        if (!result.IsSuccess)
        {
            return GameMoveResult.InvalidMove(result.Reason);
        }

        // Save updated state to DB
        IGameState state = engine.GetState();
        await _gameLogicFactory.SaveGameState(gameSession.MinigameType, request.GameId, state);

        // Notify connected players about the move via real-time channel
        if (engine.IsFinished)
        {
            await FinishGameSessionAsync(engine, gameSession.MinigameType);
            await _gameNotifier.NotifyGameFinished(request.GameId, state, engine.Winner);
        }
        else
        {
            await _gameNotifier.NotifyMoveMade(request.GameId, playerId, state);
        }

        return GameMoveResult.Success(new GameMoveResponse
        {
            Success = true,
            MinigameType = state.MinigameType,
            State = state,
            IsFinished = engine.IsFinished,
            Winner = engine.Winner
        });
    }

    public async Task<GameStateResult> GetGameStateAsync(Guid gameId, int minigameType)
    {
        _logger.LogInformation("Getting game state: GameId={GameId}, Type={Type}", gameId, minigameType);

        // Load engine from DB state
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(minigameType, gameId);
        if (engine == null)
        {
            _logger.LogWarning("Game not found: GameId={GameId}", gameId);
            return GameStateResult.NotFound("Game not found");
        }

        // Check if game should be finished
        var finishResult = await CheckFinish(engine, minigameType);
        if (finishResult != null)
        {
            return finishResult;
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

        // Check if game should be finished
        var finishResult = await CheckFinish(engine, gameSession.MinigameType);
        if (finishResult != null)
        {
            return ActiveGameResult.Success(new ActiveGameResponse
            {
                GameId = finishResult.Response!.GameId,
                Players = finishResult.Response.Players,
                IsFinished = true,
                Winner = finishResult.Response.Winner,
                State = (IGameState)finishResult.Response.State
            });
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

    public async Task<GameStateResult> FinishGameAsync(Guid gameId, int minigameType)
    {
        IGameEngine? engine = await _gameLogicFactory.LoadGameEngine(minigameType, gameId);
        if (engine == null)
        {
            return GameStateResult.NotFound("Game not found");
        }

        return await FinishGameSessionAsync(engine, minigameType);
    }

    private async Task<GameStateResult?> CheckFinish(IGameEngine engine, int minigameType)
    {
        if (engine.NeedToFinish())
        {
            return await FinishGameSessionAsync(engine, minigameType);
        }

        return null;
    }

    private async Task<GameStateResult> FinishGameSessionAsync(IGameEngine engine, int minigameType)
    {
        GameSession? gameSession = await _gameSessionRepository.GetByIdAsync(engine.GameId);
        if (gameSession != null)
        {
            gameSession.IsFinished = true;
            gameSession.IsActive = false;
            await _gameSessionRepository.UpdateAsync(gameSession);
        }

        IGameState state = engine.GetState();
        state.IsFinished = true;
        await _gameLogicFactory.SaveGameState(minigameType, engine.GameId, state);

        // Award scores to players based on game outcome (supports draws, participation, etc.)
        var playerScores = engine.GetPlayerScores();
        foreach (var (playerId, points) in playerScores)
        {
            await _userScoreService.IncrementScoreAsync(playerId, minigameType, points).ConfigureAwait(false);
        }

        // Notify connected players that the game has finished
        await _gameNotifier.NotifyGameFinished(engine.GameId, state, engine.Winner);

        return GameStateResult.Success(new GameStateResponse
        {
            GameId = engine.GameId,
            MinigameType = minigameType,
            Players = engine.Players,
            IsFinished = true,
            Winner = engine.Winner,
            State = state
        });
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
    public IReadOnlyList<string> Players { get; init; } = Array.Empty<string>();
    public bool IsFinished { get; init; }
    public string? Winner { get; init; }
    public IGameState State { get; init; } = null!;
}
