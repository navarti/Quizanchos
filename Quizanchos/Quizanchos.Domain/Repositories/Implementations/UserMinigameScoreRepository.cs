using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class UserMinigameScoreRepository : EntityRepositoryBase<Guid, UserMinigameScore>, IUserMinigameScoreRepository
{
    public UserMinigameScoreRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserMinigameScore?> FindByUserAndTypeAsync(string userId, int type)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.ApplicationUserId == userId && s.MinigameType == type).ConfigureAwait(false);
    }
}
