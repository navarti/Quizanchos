using System.Collections.Immutable;
using System.Text.Json;
using Quizanchos.Core;
using Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

namespace Quizanchos.Plugin.CaravanMultiplayer.Services;

public sealed class CaravanMpEngineFactory
{
    private readonly IGameStatePersistence _persistence;

    public CaravanMpEngineFactory(IGameStatePersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<GameEngine<CaravanMpState, CaravanMpMove>> CreateAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int seed,
        Dictionary<string, string>? nicknames)
    {
        var logic = new CaravanMpLogic(seed, nicknames);
        var engine = new GameEngine<CaravanMpState, CaravanMpMove>(logic, gameId, playerIds);

        await _persistence.CreateAsync(
            gameId,
            CaravanMpConstants.MinigameTypeId,
            playerIds.ToArray(),
            JsonSerializer.Serialize(engine.State));

        return engine;
    }

    public async Task<GameEngine<CaravanMpState, CaravanMpMove>?> LoadAsync(Guid gameId)
    {
        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            return null;
        }

        var state = JsonSerializer.Deserialize<CaravanMpState>(loaded.StateJson);
        if (state is null)
        {
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = state.IsFinished || loaded.IsFinished;
        if (!string.IsNullOrEmpty(loaded.Winner))
        {
            state.Winner = loaded.Winner;
        }

        var logic = new CaravanMpLogic();
        return new GameEngine<CaravanMpState, CaravanMpMove>(logic, state);
    }

    public async Task SaveAsync(Guid gameId, CaravanMpState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
