using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IGameSessionStateRepository
{
    Task<GameSessionState?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task<GameSessionState> CreateAsync(GameSessionState state);
    Task UpdateAsync(GameSessionState state);
}
