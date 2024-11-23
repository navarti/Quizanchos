using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class QuizCardIntRepository : EntityRepositoryBase<Guid, QuizCardInt>, IQuizCardIntRepository
{
    public QuizCardIntRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<QuizCardInt?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex)
    {
        return await _dbSet
            .Include(q => q.Option1)
            .Include(q => q.Option2)
            .FirstOrDefaultAsync(q => q.SingleGameSession.Id == gameSessionid && q.CardIndex == cardIndex);
    }
}
