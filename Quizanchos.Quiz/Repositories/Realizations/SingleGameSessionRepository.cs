using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Repositories.Interfaces;

namespace Quizanchos.Quiz.Repositories.Realizations;

public class SingleGameSessionRepository : EntityRepositoryBase<Guid, SingleGameSession>, ISingleGameSessionRepository
{
    public SingleGameSessionRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SingleGameSession?> FindAliveGameSessionForUser(string userId)
    {
        return _dbSet.FirstOrDefaultAsync(s => !s.IsFinished && s.ApplicationUser.Id == userId);
    }

    public Task<SingleGameSession?> FindAliveGameSessionForUserIncluding(string userId)
    {
        return _dbSet
            .Include(s => s.ApplicationUser)
            .Include(s => s.QuizCategory)
            .FirstOrDefaultAsync(s => !s.IsFinished && s.ApplicationUser.Id == userId);
    }

    public async Task<SingleGameSession> GetByIdIncluding(Guid id)
    {
        return await _dbSet
            .Include(s => s.ApplicationUser)
            .Include(s => s.QuizCategory)
            .FirstOrDefaultAsync(s => s.Id == id) ?? throw HandledExceptionFactory.CreateIdNotFoundException(id);
    }
}
