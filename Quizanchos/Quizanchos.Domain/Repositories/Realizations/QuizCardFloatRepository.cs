using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class QuizCardFloatRepository : EntityRepositoryBase<Guid, QuizCardFloat>, IQuizCardFloatRepository
{
    public QuizCardFloatRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<QuizCardFloat?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex)
    {
        return await _dbSet
            .Include(q => q.Option1)
            .Include(q => q.Option2)
            .FirstOrDefaultAsync(q => q.SingleGameSession.Id == gameSessionid && q.CardIndex == cardIndex);
    }
}
