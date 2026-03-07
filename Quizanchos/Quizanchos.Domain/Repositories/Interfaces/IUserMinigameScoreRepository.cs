using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IUserMinigameScoreRepository : IEntityRepository<Guid, UserMinigameScore>
{
    Task<UserMinigameScore?> FindByUserAndTypeAsync(string userId, Quizanchos.Common.Enums.MinigameType type);
}
