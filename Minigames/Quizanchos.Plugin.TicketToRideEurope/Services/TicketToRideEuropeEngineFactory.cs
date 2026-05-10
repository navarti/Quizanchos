using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Plugin.TicketToRideEurope.GameLogic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Plugin.TicketToRideEurope.Services;

public class TicketToRideEuropeEngineFactory
{
    private readonly ILogger<TicketToRideEuropeEngineFactory> _logger;
    private readonly IGameStatePersistence _persistence;

    public TicketToRideEuropeEngineFactory(
        ILogger<TicketToRideEuropeEngineFactory> logger,
        IGameStatePersistence persistence)
    {
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>> CreateEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        IReadOnlyDictionary<string, string>? playerNicknames = null)
    {
        _logger.LogInformation(
            "Creating TicketToRideEurope engine: GameId={GameId}, Players={PlayerCount}",
            gameId, playerIds.Length);

        var logic = new TicketToRideEuropeLogic();
        var engine = new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, gameId, playerIds);

        if (playerNicknames is { Count: > 0 })
        {
            engine.State.PlayerNicknames = new Dictionary<string, string>(playerNicknames);
        }

        await _persistence.CreateAsync(
            gameId,
            TicketToRideEuropeState.MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(engine.State));

        _logger.LogInformation("TicketToRideEurope engine created for GameId={GameId}", gameId);
        return engine;
    }

    public async Task<GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading TicketToRideEurope engine for GameId={GameId}", gameId);

        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            _logger.LogWarning("TicketToRideEurope state not found for GameId={GameId}", gameId);
            return null;
        }

        var state = JsonSerializer.Deserialize<TicketToRideEuropeState>(loaded.StateJson);
        if (state is null)
        {
            _logger.LogWarning("TicketToRideEurope state failed to deserialize for GameId={GameId}", gameId);
            return null;
        }

        state.GameId = gameId;
        // Derive Players from PlayerStates (preserved in JSON in original turn order).
        // loaded.PlayerIds comes from EF's GameSession.Players nav collection which has no
        // guaranteed ordering — using it would scramble the turn rotation after a reload.
        state.Players = state.PlayerStates.Select(p => p.PlayerId).ToList();
        state.IsFinished = loaded.IsFinished;
        // state.Winner intentionally preserved from JSON (set during gameplay, not host-tracked).

        var logic = new TicketToRideEuropeLogic();
        return new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, state);
    }

    public async Task SaveStateAsync(Guid gameId, TicketToRideEuropeState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
