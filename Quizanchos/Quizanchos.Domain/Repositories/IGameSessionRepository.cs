using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id);
    Task<GameSession?> GetActiveByPlayerIdAsync(string playerId);
    Task<GameSession> CreateAsync(GameSession gameSession);
    Task UpdateAsync(GameSession gameSession);
    Task<bool> DeleteAsync(Guid id);
}
