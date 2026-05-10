using System.Collections.Immutable;
using System.Text.Json;
using Quizanchos.Core;
using Quizanchos.Plugin.Caravan.GameLogic;

namespace Quizanchos.Plugin.Caravan.Services;

public sealed class CaravanEngineFactory
{
    private readonly IGameStatePersistence _persistence;

    public CaravanEngineFactory(IGameStatePersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<GameEngine<CaravanState, CaravanMove>> CreateAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int seed)
    {
        var logic = new CaravanLogic(seed);
        var engine = new GameEngine<CaravanState, CaravanMove>(logic, gameId, playerIds);

        // The host only knows about human players, so persist just those (the AI is internal).
        await _persistence.CreateAsync(
            gameId,
            CaravanConstants.MinigameTypeId,
            playerIds.ToArray(),
            JsonSerializer.Serialize(engine.State));

        return engine;
    }

    public async Task<GameEngine<CaravanState, CaravanMove>?> LoadAsync(Guid gameId)
    {
        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            return null;
        }

        var state = JsonSerializer.Deserialize<CaravanState>(loaded.StateJson);
        if (state is null)
        {
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = state.IsFinished || loaded.IsFinished;
        // Don't let a null loaded.Winner overwrite the JSON winner: when the internal AI wins,
        // its id ("__caravan_ai__") isn't in GameSession.Players so GameService can't persist
        // it to GameSession.WinnerId, leaving loaded.Winner null while the JSON has the truth.
        if (!string.IsNullOrEmpty(loaded.Winner))
        {
            state.Winner = loaded.Winner;
        }

        var logic = new CaravanLogic();
        return new GameEngine<CaravanState, CaravanMove>(logic, state);
    }

    public async Task SaveAsync(Guid gameId, CaravanState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
