using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Game2048.GameLogic;

namespace Quizanchos.Game2048.Services;

public class Game2048StateService
{
    private readonly IGameSessionStateRepository _repository;

    public Game2048StateService(IGameSessionStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Game2048State?> LoadStateAsync(Guid gameSessionId)
    {
        GameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        Game2048State? state = JsonSerializer.Deserialize<Game2048State>(sessionState.StateJson);
        if (state == null)
            return null;

        state.GameId = sessionState.GameSessionId;
        state.Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        state.Winner = sessionState.GameSession.WinnerId;

        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, Game2048State state)
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
        Game2048State state)
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
