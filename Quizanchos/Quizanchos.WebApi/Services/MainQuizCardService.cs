using AutoMapper;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services;

public class MainQuizCardService
{
    private readonly IMapper _mapper;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly QuizCardFloatService _featureFloatService;
    private readonly QuizCardIntService _featureIntService;

    public MainQuizCardService(
        IMapper mapper,
        IQuizCategoryRepository quizCategoryRepository,
        QuizCardFloatService featureFloatService,
        QuizCardIntService featureIntService)
    {
        _mapper = mapper;
        _quizCategoryRepository = quizCategoryRepository;
        _featureFloatService = featureFloatService;
        _featureIntService = featureIntService;
    }

    public async Task<QuizCardDtoAbstract> GetCardDtoForSession(SingleGameSession gameSession, int cardIndex)
    {
        QuizCardAbstract card = await GetCardForSession(gameSession, cardIndex);
        if (gameSession.IsFinished)
        {
            return MapQuizCardDtoWithAnswer(card);
        }
        return MapQuizCardDto(card);
    }

    public async Task<QuizCardAbstract> GetCardForSession(SingleGameSession gameSession, int cardIndex)
    {
        QuizCardAbstract? card = await FindCardForSession(gameSession, cardIndex);
        _ = card ?? throw HandledExceptionFactory.Create($"Could not find card with index {cardIndex} for session {gameSession.Id}");
        return card;
    }

    public async Task<QuizCardAbstract?> FindCardForSession(SingleGameSession gameSession, int cardIndex)
    {
        IQuizCardService quizCardService = GetQuizCardService(gameSession.QuizCategory.FeatureType);
        QuizCardAbstract? card = await quizCardService.FindCardForSession(gameSession.Id, cardIndex);
        if (card is null)
        {
            return null;
        }
        return card;
    }

    public async Task<QuizCardDtoAbstract> CreateNextCardForSession(SingleGameSession gameSession)
    {
        IQuizCardService quizCardService = GetQuizCardService(gameSession.QuizCategory.FeatureType);
        QuizCardAbstract card = await quizCardService.CreateCardForSession(gameSession);
        return MapQuizCardDto(card);
    }

    public async Task<(QuizCardDtoAbstract QuizCardDto, bool IsCorrect)> PickAnswerForSession(SingleGameSession gameSession, int optionPicked)
    {
        IQuizCardService quizCardService = GetQuizCardService(gameSession.QuizCategory.FeatureType);
        (QuizCardAbstract QuizCard, bool IsCorrect) result = await quizCardService.PickAnswerForSession(gameSession.Id, gameSession.CurrentCardIndex, optionPicked);
        return (MapQuizCardDtoWithAnswer(result.QuizCard), result.IsCorrect);
    }

    private IQuizCardService GetQuizCardService(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.Float => _featureFloatService,
            FeatureType.Int => _featureIntService,
            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(FeatureType)}: {featureType}")
        };
    }

    // TODO: move these 2 methods to services
    private QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard)
    {
        return quizCard switch
        {
            // TODO: fix
            //QuizCardFloat quizCardFloat => _mapper.Map<QuizCardFloatDto>(quizCardFloat),
            QuizCardFloat quizCardFloat => new QuizCardFloatDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.OptionPicked, quizCardFloat.CreationTime, quizCardFloat.Options.Select(o => o.QuizEntity.Id).ToArray()),
            //QuizCardInt quizCardInt => _mapper.Map<QuizCardIntDto>(quizCardInt),
            QuizCardInt quizCardInt => new QuizCardIntDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.OptionPicked, quizCardInt.CreationTime, quizCardInt.Options.Select(o => o.QuizEntity.Id).ToArray()),
            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}")
        };
    }

    private QuizCardDtoAbstract MapQuizCardDtoWithAnswer(QuizCardAbstract quizCard)
    {
        return quizCard switch
        {
            // TODO: fix
            //QuizCardFloat quizCardFloat => _mapper.Map<QuizCardFloatDto>(quizCardFloat),
            QuizCardFloat quizCardFloat => new QuizCardFloatWithAnswerDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.OptionPicked, quizCardFloat.CreationTime, quizCardFloat.Options.Select(o => o.QuizEntity.Id).ToArray(), 
                quizCardFloat.CorrectOption, quizCardFloat.Options.Select(o => o.Value.Value).ToArray()),

            //QuizCardInt quizCardInt => _mapper.Map<QuizCardIntDto>(quizCardInt),
            QuizCardInt quizCardInt => new QuizCardIntWithAnswerDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.OptionPicked, quizCardInt.CreationTime, quizCardInt.Options.Select(o => o.QuizEntity.Id).ToArray(),
                quizCardInt.CorrectOption, quizCardInt.Options.Select(o => o.Value.Value).ToArray()),

            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}")
        };
    }
}
