using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface ISingleGameSessionRepository : IEntityRepository<Guid, SingleGameSession>
{
    Task<SingleGameSession> GetByIdIncluding(Guid id);
    Task<SingleGameSession?> FindAliveGameSessionForUser(string userId);
    Task<SingleGameSession?> FindAliveGameSessionForUserIncluding(string userId);
}
