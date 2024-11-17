using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Services;

public class QuizCardFloatService : IQuizCardService
{
    private readonly IQuizCardFloatRepository _quizCardFloatRepository;

    public QuizCardFloatService(IQuizCardFloatRepository quizCardFloatRepository)
    {
        _quizCardFloatRepository = quizCardFloatRepository;
    }

    public Task GetCardForSession(Guid gameSessionid, int cardIndex)
    {
        return _quizCardFloatRepository.GetCardForSession(gameSessionid, cardIndex);
    }
}
