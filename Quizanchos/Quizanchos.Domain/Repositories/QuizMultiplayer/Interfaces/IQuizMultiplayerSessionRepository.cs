using Quizanchos.Domain.Entities.QuizMultiplayer;

namespace Quizanchos.Domain.Repositories.QuizMultiplayer.Interfaces;

public interface IQuizMultiplayerSessionRepository
{
    Task<QuizMultiplayerSessionState?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task<QuizMultiplayerSessionState> CreateAsync(QuizMultiplayerSessionState state);
    Task UpdateAsync(QuizMultiplayerSessionState state);
}
