using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Services;

public class QuizCardFloatService : IQuizCardService
{
    private readonly IQuizCardFloatRepository _quizCardFloatRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public QuizCardFloatService(IQuizCardFloatRepository quizCardFloatRepository, IFeatureFloatRepository featureFloatRepository)
    {
        _quizCardFloatRepository = quizCardFloatRepository;
        _featureFloatRepository = featureFloatRepository;
    }

    public async Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession)
    {
        FeatureFloat? featureFloat1 = await _featureFloatRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
            ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

        FeatureFloat? featureFloat2 = await _featureFloatRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
            ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

        QuizCardFloat quizCardFloat = new QuizCardFloat
        {
            SingleGameSession = gameSession,
            CardIndex = gameSession.CurrentCardIndex,
            Option1 = featureFloat1,
            Option2 = featureFloat2,
            CorrectOption = 0,
            OptionPicked = null,
            CreationTime = DateTime.UtcNow
        };

        return await _quizCardFloatRepository.Create(quizCardFloat);
    }

    public async Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _quizCardFloatRepository.FindCardForSessionIncluding(gameSessionid, cardIndex);
    }

    public async Task<QuizCardAbstract> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        return await _quizCardFloatRepository.PickAnswerForSession(gameSessionid, cardIndex, optionPicked);
    }
}
