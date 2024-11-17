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

    public Task GetCardForSession(Guid gameSessionid, int cardIndex)
    {
        return _quizCardIntRepository.GetCardForSession(gameSessionid, cardIndex);
    }
}
