using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Game2048.GameLogic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Game2048.Services;

public class Game2048EngineFactory
{
    private const int MinigameTypeId = 2;

    private readonly ILogger<Game2048EngineFactory> _logger;
    private readonly IGameStatePersistence _persistence;

    public Game2048EngineFactory(
        ILogger<Game2048EngineFactory> logger,
        IGameStatePersistence persistence)
    {
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<GameEngine<Game2048State, Game2048Move>> CreateGame2048EngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int size)
    {
        _logger.LogInformation("Creating 2048 engine with Size={Size}", size);

        var logic = new Game2048Logic(size);
        var engine = new GameEngine<Game2048State, Game2048Move>(logic, gameId, playerIds);

        await _persistence.CreateAsync(
            gameId,
            MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(engine.State));

        _logger.LogInformation(
            "2048 engine created. Score={Score}, BestTile={BestTile}",
            engine.State.Score,
            engine.State.BestTile);

        return engine;
    }

    public async Task<GameEngine<Game2048State, Game2048Move>?> LoadGame2048EngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading 2048 engine for GameId={GameId}", gameId);

        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            _logger.LogWarning("2048 state not found for GameId={GameId}", gameId);
            return null;
        }

        var state = JsonSerializer.Deserialize<Game2048State>(loaded.StateJson);
        if (state is null)
        {
            _logger.LogWarning("2048 state could not be deserialized for GameId={GameId}", gameId);
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        state.Winner = loaded.Winner;

        var logic = new Game2048Logic(state.Size);
        var engine = new GameEngine<Game2048State, Game2048Move>(logic, state);

        _logger.LogInformation(
            "2048 engine loaded. Score={Score}, BestTile={BestTile}, MoveCount={MoveCount}",
            state.Score,
            state.BestTile,
            state.MoveCount);

        return engine;
    }

    public async Task SaveGame2048StateAsync(Guid gameId, Game2048State state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
