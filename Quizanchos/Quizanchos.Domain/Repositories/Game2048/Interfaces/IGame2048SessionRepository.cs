using Quizanchos.Domain.Entities.Game2048;

namespace Quizanchos.Domain.Repositories.Game2048.Interfaces;

public interface IGame2048SessionRepository
{
    Task<Game2048SessionState?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task<Game2048SessionState> CreateAsync(Game2048SessionState state);
    Task UpdateAsync(Game2048SessionState state);
}
