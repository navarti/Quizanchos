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
        if (sessionState == null)
            return null;

        TicketToRideEuropeState? state = JsonSerializer.Deserialize<TicketToRideEuropeState>(sessionState.StateJson);
        if (state == null)
            return null;

        state.GameId = sessionState.GameSessionId;
        state.Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        state.Winner = sessionState.GameSession.WinnerId;

        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, TicketToRideEuropeState state)
    {
        GameSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (existingState == null)
        {
            throw new InvalidOperationException($"GameSessionState not found for GameSessionId: {gameSessionId}");
        }

        existingState.StateJson = JsonSerializer.Serialize(state);
        existingState.UpdatedAt = DateTime.UtcNow;

        existingState.GameSession.IsFinished = state.IsFinished;
        if (!string.IsNullOrEmpty(state.Winner))
        {
            existingState.GameSession.WinnerId = state.Winner;
            existingState.GameSession.FinishedAt = DateTime.UtcNow;
        }
        if (state.IsFinished)
        {
            existingState.GameSession.IsActive = false;
        }

        await _repository.UpdateAsync(existingState);
    }

    public async Task<GameSessionState> CreateInitialStateAsync(
        GameSession gameSession,
        TicketToRideEuropeState state)
    {
        var sessionState = new GameSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            MinigameType = gameSession.MinigameType,
            StateJson = JsonSerializer.Serialize(state),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(sessionState);
        return sessionState;
    }
}
