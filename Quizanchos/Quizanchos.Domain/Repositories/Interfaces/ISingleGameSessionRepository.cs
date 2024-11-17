using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface ISingleGameSessionRepository : IEntityRepository<Guid, SingleGameSession>
{
    Task<SingleGameSession?> FindAliveGameSessionForUser(string userId);
}
