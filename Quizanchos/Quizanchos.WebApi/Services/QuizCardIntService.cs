using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Services;

public class QuizCardIntService : IQuizCardService
{
    private readonly IQuizCardIntRepository _quizCardIntRepository;

    public QuizCardIntService(IQuizCardIntRepository quizCardIntRepository)
    {
        _quizCardIntRepository = quizCardIntRepository;
    }

    public async Task<QuizCardAbstract> GetCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _quizCardIntRepository.GetCardForSession(gameSessionid, cardIndex);
    }
}
