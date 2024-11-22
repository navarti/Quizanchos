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
        QuizCardAbstract? card = await FindCardForSession(gameSession, cardIndex);
        _ = card ?? throw HandledExceptionFactory.Create($"Could not find card with index {cardIndex} for session {gameSession.Id}");
        return MapQuizCardDto(card);
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

    private IQuizCardService GetQuizCardService(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.Float => _featureFloatService,
            FeatureType.Int => _featureIntService,
            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(FeatureType)}: {featureType}")
        };
    }

    private QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard)
    {
        return quizCard switch
        {
            // TODO: fix
            //QuizCardFloat quizCardFloat => _mapper.Map<QuizCardFloatDto>(quizCardFloat),
            QuizCardFloat quizCardFloat => new QuizCardFloatDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.Option1.Value.Value, quizCardFloat.Option2.Value.Value, quizCardFloat.OptionPicked ?? -1, quizCardFloat.CreationTime),
            //QuizCardInt quizCardInt => _mapper.Map<QuizCardIntDto>(quizCardInt),
            QuizCardInt quizCardInt => new QuizCardIntDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.Option1.Value.Value, quizCardInt.Option2.Value.Value, quizCardInt.OptionPicked ?? -1, quizCardInt.CreationTime),
            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}")
        };
    }
}
