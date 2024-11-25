using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.WebApi.Services;

public class SessionTerminatorService
{
    private readonly ISingleGameSessionRepository _singleGameSessionRepository;
    private readonly MainQuizCardService _mainQuizCardService;

    public SessionTerminatorService(ISingleGameSessionRepository singleGameSessionRepository, MainQuizCardService mainQuizCardService)
    {
        _singleGameSessionRepository = singleGameSessionRepository;
        _mainQuizCardService = mainQuizCardService;
    }

    public async Task TerminateSessionIfNeeded(SingleGameSession gameSession)
    {
        QuizCardAbstract? card = await _mainQuizCardService.FindCardForSession(gameSession, gameSession.CurrentCardIndex);
        if(card is null)
        {
            return;
        }

        if(!HasExpiredTime(card.CreationTime, gameSession.SecondsPerCard))
        {
            return;
        }

        gameSession.IsTerminatedByTime = true;
        gameSession.IsFinished = true;
        await _singleGameSessionRepository.Update(gameSession);
    }

    private bool HasExpiredTime(DateTime creationTime, int secondsToAdd)
    {
        return DateTime.UtcNow > creationTime.AddSeconds(secondsToAdd);
    }
}
