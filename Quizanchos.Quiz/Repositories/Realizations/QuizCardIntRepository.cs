using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Repositories.Interfaces;

namespace Quizanchos.Quiz.Repositories.Realizations;

public class QuizCardIntRepository : EntityRepositoryBase<Guid, QuizCardInt>, IQuizCardIntRepository
{
    public QuizCardIntRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<QuizCardInt?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex)
    {
        QuizCardInt? quizCardInt = await _dbContext.QuizCardInts
            .FirstOrDefaultAsync(q => q.SingleGameSession.Id == gameSessionid && q.CardIndex == cardIndex);
        return quizCardInt;
    }

    public async Task<QuizCardInt> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        QuizCardInt? card = await FindCardForSessionIncluding(gameSessionid, cardIndex);
        _ = card ?? throw HandledExceptionFactory.Create("Card not found");

        if (card.OptionPicked is not null)
        {
            throw HandledExceptionFactory.Create("Card already answered");
        }

        card.OptionPicked = optionPicked;
        await _dbContext.SaveChangesAsync();
        return card;
    }
}
