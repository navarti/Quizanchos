using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Game2048.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.Game2048.Services;

public class Game2048EngineFactory
{
    private const int Game2048MinigameTypeId = 2;
    private readonly ILogger<Game2048EngineFactory> _logger;
    private readonly Game2048StateService _stateService;
    private readonly IGameSessionRepository _gameSessionRepository;

    public Game2048EngineFactory(
        ILogger<Game2048EngineFactory> logger,
        Game2048StateService stateService,
        IGameSessionRepository gameSessionRepository)
    {
        _logger = logger;
        _stateService = stateService;
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<GameEngine<Game2048State, Game2048Move>> CreateGame2048EngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int size)
    {
        _logger.LogInformation("Creating 2048 engine with Size={Size}", size);

        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = Game2048MinigameTypeId,
            IsActive = true,
            IsFinished = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (string playerId in playerIds)
        {
            gameSession.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _gameSessionRepository.CreateAsync(gameSession);

        Game2048Logic logic = new Game2048Logic(size);
        GameEngine<Game2048State, Game2048Move> engine = new GameEngine<Game2048State, Game2048Move>(logic, gameId, playerIds);

        Game2048State state = engine.State;

        await _stateService.CreateInitialStateAsync(gameSession, size, state.Board);

        _logger.LogInformation("2048 engine created. Score={Score}, BestTile={BestTile}", state.Score, state.BestTile);

        return engine;
    }

    public async Task<GameEngine<Game2048State, Game2048Move>?> LoadGame2048EngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading 2048 engine for GameId={GameId}", gameId);

        Game2048State? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("2048 state not found for GameId={GameId}", gameId);
            return null;
        }

        Game2048Logic logic = new Game2048Logic(state.Size);
        GameEngine<Game2048State, Game2048Move> engine = new GameEngine<Game2048State, Game2048Move>(logic, state);

        _logger.LogInformation("2048 engine loaded. Score={Score}, BestTile={BestTile}, MoveCount={MoveCount}",
            state.Score, state.BestTile, state.MoveCount);

        return engine;
    }

    public async Task SaveGame2048StateAsync(Guid gameId, Game2048State state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
