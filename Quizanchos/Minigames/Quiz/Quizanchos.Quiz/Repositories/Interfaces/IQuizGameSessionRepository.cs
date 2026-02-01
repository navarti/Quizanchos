using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.Quiz.Repositories.Interfaces;

public interface IQuizGameSessionRepository
{
    Task<QuizGameSessionState?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task<QuizGameSessionState> CreateAsync(QuizGameSessionState state);
    Task UpdateAsync(QuizGameSessionState state);
    Task<QuizSessionCard?> GetCardByIndexAsync(Guid quizGameSessionStateId, int cardIndex);
    Task AddCardAsync(QuizSessionCard card);
    Task UpdateCardAsync(QuizSessionCard card);
    Task AddCardAnswerAsync(QuizSessionCardAnswer answer);
    Task UpdatePlayerScoreAsync(Guid quizGameSessionStateId, string applicationUserId, int score);
}
