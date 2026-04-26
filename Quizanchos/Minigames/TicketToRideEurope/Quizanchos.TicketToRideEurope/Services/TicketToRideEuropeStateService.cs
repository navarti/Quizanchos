using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.TicketToRideEurope.GameLogic;

namespace Quizanchos.TicketToRideEurope.Services;

public class TicketToRideEuropeStateService
{
    private readonly IGameSessionStateRepository _repository;

    public TicketToRideEuropeStateService(IGameSessionStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TicketToRideEuropeState?> LoadStateAsync(Guid gameSessionId)
    {
        GameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null) return null;

        TicketToRideEuropeState? state = JsonSerializer.Deserialize<TicketToRideEuropeState>(sessionState.StateJson);
        if (state == null) return null;

        state.GameId = sessionState.GameSessionId;
        // Derive Players from PlayerStates (preserved in JSON in original turn order).
        // GameSession.Players is an EF navigation collection with no guaranteed ordering,
        // so reading from it would scramble the turn rotation after a reload, making
        // Players[CurrentPlayerIndex] disagree with PlayerStates[CurrentPlayerIndex] and
        // causing every move to be rejected as "Player can not commit now".
        state.Players = state.PlayerStates.Select(p => p.PlayerId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, TicketToRideEuropeState state)
    {
        GameSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (existingState == null)
            throw new InvalidOperationException($"GameSessionState not found for GameSessionId: {gameSessionId}");

        existingState.StateJson = JsonSerializer.Serialize(state);
        existingState.UpdatedAt = DateTime.UtcNow;

        existingState.GameSession.IsFinished = state.IsFinished;
        if (state.IsFinished)
        {
            existingState.GameSession.IsActive = false;
            existingState.GameSession.FinishedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(state.Winner))
            {
                existingState.GameSession.WinnerId = state.Winner;
            }
        }

        await _repository.UpdateAsync(existingState);
    }

    public async Task CreateInitialStateAsync(Guid gameSessionId, TicketToRideEuropeState state)
    {
        GameSessionState sessionState = new GameSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            MinigameType = state.MinigameType,
            StateJson = JsonSerializer.Serialize(state),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(sessionState);
    }
}
