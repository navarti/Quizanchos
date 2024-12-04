using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services;

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
        List<FeatureInt> featureInts = new List<FeatureInt>();
        for (int i = 0; i < gameSession.OptionCountInt; i++)
        {
            featureInts.Add(await _featureIntRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
                ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later"));
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
}
