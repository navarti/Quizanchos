using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.Quiz.Services;

public class QuizGameStateService
{
    private readonly IGameSessionStateRepository _repository;

    public QuizGameStateService(IGameSessionStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuizGameState?> LoadStateAsync(Guid gameSessionId)
    {
        GameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        QuizGameState? state = JsonSerializer.Deserialize<QuizGameState>(sessionState.StateJson);
        if (state == null)
            return null;

        state.GameId = sessionState.GameSessionId;
        state.Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        state.Winner = sessionState.GameSession.WinnerId;

        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, QuizGameState state)
    {
        GameSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        
        if (existingState == null)
        {
            throw new InvalidOperationException($"GameSessionState not found for GameSessionId: {gameSessionId}");
        }

        existingState.StateJson = JsonSerializer.Serialize(state);
        existingState.UpdatedAt = DateTime.UtcNow;

        // Update game session
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
        QuizGameState state)
    {
        GameSessionState sessionState = new GameSessionState
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
