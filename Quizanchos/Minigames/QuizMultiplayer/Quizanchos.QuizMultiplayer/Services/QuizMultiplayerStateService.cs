using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.QuizMultiplayer.GameLogic;

namespace Quizanchos.QuizMultiplayer.Services;

public class QuizMultiplayerStateService
{
    private readonly IGameSessionStateRepository _repository;

    public QuizMultiplayerStateService(IGameSessionStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuizMultiplayerGameState?> LoadStateAsync(Guid gameSessionId)
    {
        GameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        QuizMultiplayerGameState? state = JsonSerializer.Deserialize<QuizMultiplayerGameState>(sessionState.StateJson);
        if (state == null)
            return null;

        // Restore fields from the GameSession navigation
        state.GameId = sessionState.GameSessionId;
        state.Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        // state.Winner is the winning team name, already stored in StateJson;
        // do not overwrite it with GameSession.WinnerId (a user FK).

        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, QuizMultiplayerGameState state)
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
        }

        await _repository.UpdateAsync(existingState);
    }

    public async Task CreateInitialStateAsync(Guid gameSessionId, QuizMultiplayerGameState state)
    {
        var sessionState = new GameSessionState
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
