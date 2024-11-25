using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
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
                .ThenInclude(f => f.QuizEntity)
            .Include(q => q.Option2)
                .ThenInclude(f => f.QuizEntity)
            .FirstOrDefaultAsync(q => q.SingleGameSession.Id == gameSessionid && q.CardIndex == cardIndex);
    }

    public async Task<QuizCardFloat> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        QuizCardFloat? card = await FindCardForSessionIncluding(gameSessionid, cardIndex);
        _ = card ?? throw HandledExceptionFactory.Create("Card not found");

        if(card.OptionPicked is not null)
        {
            throw HandledExceptionFactory.Create("Card already answered");
        }

        card.OptionPicked = optionPicked;
        await _dbContext.SaveChangesAsync();
        return card;
    }
}
