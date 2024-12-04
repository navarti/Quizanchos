using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

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
        List<FeatureFloat> featureFloats = new List<FeatureFloat>();
        for (int i = 0; i < gameSession.OptionCountInt; i++)
        {
            featureFloats.Add(await _featureFloatRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
                ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later"));
        }

        int correctOption = CorrectOptionPicker.PickCorrectOption(featureFloats);

        QuizCardFloat quizCardFloat = new QuizCardFloat
        {
            SingleGameSession = gameSession,
            CardIndex = gameSession.CurrentCardIndex,
            Options = featureFloats,
            CorrectOption = correctOption,
            OptionPicked = null,
            CreationTime = DateTime.UtcNow
        };

        return await _quizCardFloatRepository.Create(quizCardFloat);
    }

    public async Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _quizCardFloatRepository.FindCardForSessionIncluding(gameSessionid, cardIndex);
    }

    public async Task<(QuizCardAbstract QuizCard, bool IsCorrect)> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        QuizCardFloat? quizCard = await _quizCardFloatRepository.PickAnswerForSession(gameSessionid, cardIndex, optionPicked);
        return (quizCard, quizCard.CorrectOption == optionPicked);
    }
}
