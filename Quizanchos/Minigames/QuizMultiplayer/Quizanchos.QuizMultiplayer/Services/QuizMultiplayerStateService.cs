using System.Text.Json;
using Quizanchos.Domain.Entities.QuizMultiplayer;
using Quizanchos.Domain.Repositories.QuizMultiplayer.Interfaces;
using Quizanchos.QuizMultiplayer.GameLogic;

namespace Quizanchos.QuizMultiplayer.Services;

public class QuizMultiplayerStateService
{
    private readonly IQuizMultiplayerSessionRepository _repository;

    public QuizMultiplayerStateService(IQuizMultiplayerSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuizMultiplayerGameState?> LoadStateAsync(Guid gameSessionId)
    {
        QuizMultiplayerSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
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
        QuizMultiplayerSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (existingState == null)
            throw new InvalidOperationException($"QuizMultiplayerSessionState not found for GameSessionId: {gameSessionId}");

        existingState.StateJson = JsonSerializer.Serialize(state);

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
        var sessionState = new QuizMultiplayerSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            StateJson = JsonSerializer.Serialize(state),
            CreationTime = DateTime.UtcNow
        };

        await _repository.CreateAsync(sessionState);
    }
}
