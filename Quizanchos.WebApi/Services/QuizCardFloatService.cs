using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;
using Quizanchos.Domain.Repositories.Realizations;

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
        float? previousValue = null;

        List<FeatureFloat> featureFloats = new List<FeatureFloat>();
        for (int i = 0; i < gameSession.OptionCountInt; i++)
        {
            FeatureFloat feature = await _featureFloatRepository.FindRandomByCategory(gameSession.QuizCategory.Id, gameSession.GameLevel.ToCoefficient(), previousValue)
                ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

            previousValue = feature.Value.Value;
            featureFloats.Add(feature);
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

    public QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard)
    {
        if (quizCard is not QuizCardFloat quizCardFloat)
        {
            throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}");
        }
        return new QuizCardFloatDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.OptionPicked, quizCardFloat.CreationTime, quizCardFloat.Options.Select(o => o.QuizEntity.Id).ToArray());
    }

    public QuizCardDtoAbstract MapQuizCardDtoWithAnswer(QuizCardAbstract quizCard)
    {
        if (quizCard is not QuizCardFloat quizCardFloat)
        {
            throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}");
        }
        return new QuizCardFloatWithAnswerDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.OptionPicked, quizCardFloat.CreationTime, quizCardFloat.Options.Select(o => o.QuizEntity.Id).ToArray(),
                quizCardFloat.CorrectOption, quizCardFloat.Options.Select(o => o.Value.Value).ToArray());
    }
}
