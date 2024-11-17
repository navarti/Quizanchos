using Quizanchos.Domain.Entities.Abstractions;
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

    public async Task<QuizCardAbstract> GetCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _quizCardFloatRepository.GetCardForSession(gameSessionid, cardIndex);
    }
}
