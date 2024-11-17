using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class QuizCardIntRepository : EntityRepositoryBase<Guid, QuizCardInt>, IQuizCardIntRepository
{
    public QuizCardIntRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<QuizCardInt> GetCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _dbSet.FirstOrDefaultAsync(q => q.SingleGameSession.Id == gameSessionid && q.CardIndex == cardIndex)
            ?? throw HandledExceptionFactory.Create($"No card with index {cardIndex} for gameSessionid {gameSessionid} found");
    }
}
