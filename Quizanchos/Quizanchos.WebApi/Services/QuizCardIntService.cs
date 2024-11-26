using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
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
        FeatureInt? featureInt1 = await _featureIntRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
            ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

        FeatureInt? featureInt2 = await _featureIntRepository.FindRandomByCategory(gameSession.QuizCategory.Id)
            ?? throw HandledExceptionFactory.Create("Could not find any records for this category. Try again later");

        int correctOption = CorrectOptionPicker.PickCorrectOption([featureInt1, featureInt2]);

        QuizCardInt quizCardInt = new QuizCardInt
        {
            SingleGameSession = gameSession,
            CardIndex = gameSession.CurrentCardIndex,
            Option1 = featureInt1,
            Option2 = featureInt2,
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

    public async Task<QuizCardAbstract> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked)
    {
        return await _quizCardIntRepository.PickAnswerForSession(gameSessionid, cardIndex, optionPicked);
    }
}
