using Quizanchos.Common.Util;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Entities.Abstractions;
using Quizanchos.Quiz.Repositories.Interfaces;
using Quizanchos.Quiz.Dto.Abstractions;
using Quizanchos.Quiz.Dto;
using Quizanchos.Quiz.Services.Interfaces;
using Quizanchos.Quiz.Util;

namespace Quizanchos.Quiz.Services;

public class QuizCardIntService : IQuizCardService
{
    private readonly IQuizCardIntRepository _quizCardIntRepository;
    private readonly IFeatureIntRepository _featureIntRepository;

    public QuizCardIntService(IQuizCardIntRepository quizCardIntRepository, IFeatureIntRepository featureIntRepository)
    {
        _quizCardIntRepository = quizCardIntRepository;
        _featureIntRepository = featureIntRepository;
    }

    public async Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession)
    {
        int? previousValue = null;

        List<FeatureInt> featureInts = new List<FeatureInt>();
        for (int i = 0; i < gameSession.OptionCountInt; i++)
        {
            FeatureInt feature = await _featureIntRepository.FindRandomByCategory(gameSession.QuizCategory.Id, gameSession.GameLevel.ToCoefficient(), previousValue)
                ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

            previousValue = feature.Value.Value;
            featureInts.Add(feature);
        }

        int correctOption = CorrectOptionPicker.PickCorrectOption(featureInts);

        QuizCardInt quizCardInt = new QuizCardInt
        {
            SingleGameSession = gameSession,
            CardIndex = gameSession.CurrentCardIndex,
            Options = featureInts,
            CorrectOption = correctOption,
            OptionPicked = null,
            CreationTime = DateTime.UtcNow
        };

        return await _quizCardIntRepository.Create(quizCardInt);
    }

    public async Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex)
    {
        return await _quizCardIntRepository.FindCardForSessionIncluding(gameSessionid, cardIndex);
    }

    public async Task<(QuizCardAbstract QuizCard, bool IsCorrect)> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        QuizCardInt? quizCard = await _quizCardIntRepository.PickAnswerForSession(gameSessionid, cardIndex, optionPicked);
        return (quizCard, quizCard.CorrectOption == optionPicked);
    }

    public QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard)
    {
        if(quizCard is not QuizCardInt quizCardInt)
        {
            throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}");
        }

        return new QuizCardIntDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.OptionPicked, quizCardInt.CreationTime, quizCardInt.Options.Select(o => o.QuizEntity.Id).ToArray());
    }

    public QuizCardDtoAbstract MapQuizCardDtoWithAnswer(QuizCardAbstract quizCard)
    {
        if (quizCard is not QuizCardInt quizCardInt)
        {
            throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}");
        }

        return new QuizCardIntWithAnswerDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.OptionPicked, quizCardInt.CreationTime, quizCardInt.Options.Select(o => o.QuizEntity.Id).ToArray(),
                quizCardInt.CorrectOption, quizCardInt.Options.Select(o => o.Value.Value).ToArray());
    }
}
