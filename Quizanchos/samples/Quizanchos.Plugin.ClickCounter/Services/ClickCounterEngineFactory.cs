using System.Collections.Immutable;
using System.Text.Json;
using Quizanchos.Core;
using Quizanchos.Plugin.ClickCounter.GameLogic;

namespace Quizanchos.Plugin.ClickCounter.Services;

public sealed class ClickCounterEngineFactory
{
    private readonly IGameStatePersistence _persistence;

    public ClickCounterEngineFactory(IGameStatePersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<GameEngine<ClickCounterState, ClickCounterMove>> CreateAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int target)
    {
        var logic = new ClickCounterLogic(target);
        var engine = new GameEngine<ClickCounterState, ClickCounterMove>(logic, gameId, playerIds);

        await _persistence.CreateAsync(
            gameId,
            ClickCounterConstants.MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(engine.State));

        return engine;
    }

    public async Task<GameEngine<ClickCounterState, ClickCounterMove>?> LoadAsync(Guid gameId)
    {
        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            return null;
        }

        var state = JsonSerializer.Deserialize<ClickCounterState>(loaded.StateJson);
        if (state is null)
        {
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        state.Winner = loaded.Winner;

        var logic = new ClickCounterLogic(state.Target);
        return new GameEngine<ClickCounterState, ClickCounterMove>(logic, state);
    }

    public async Task SaveAsync(Guid gameId, ClickCounterState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
