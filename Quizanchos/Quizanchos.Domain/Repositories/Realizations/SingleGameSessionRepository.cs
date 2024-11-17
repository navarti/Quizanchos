using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class SingleGameSessionRepository : EntityRepositoryBase<Guid, SingleGameSession>, ISingleGameSessionRepository
{
    public SingleGameSessionRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SingleGameSession?> FindAliveGameSessionForUser(string userId)
    {
        return _dbSet
            .Include(s => s.ApplicationUser)
            .Include(s => s.QuizCategory)
            .FirstOrDefaultAsync(s => !s.IsFinished && s.ApplicationUser.Id == userId);
    }
}
