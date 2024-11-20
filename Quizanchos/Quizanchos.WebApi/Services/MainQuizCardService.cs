using AutoMapper;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Services.HelperServices;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class MainQuizCardService
{
    private readonly IMapper _mapper;
    private readonly ISingleGameSessionRepository _singleGameSessionRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly QuizCardFloatService _featureFloatService;
    private readonly QuizCardIntService _featureIntService;

    public MainQuizCardService(
        IMapper mapper,
        ISingleGameSessionRepository singleGameSessionRepository,
        IQuizCategoryRepository quizCategoryRepository,
        UserRetrieverService userRetrieverService,
        QuizCardFloatService featureFloatService,
        QuizCardIntService featureIntService)
    {
        _mapper = mapper;
        _singleGameSessionRepository = singleGameSessionRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _userRetrieverService = userRetrieverService;
        _featureFloatService = featureFloatService;
        _featureIntService = featureIntService;
    }

    public async Task<QuizCardDtoAbstract> GetCardForSession(ClaimsPrincipal claimsPrincipal, Guid sessionid, int cardIndex)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        IQuizCardService quizCardService = GetQuizCardService(gameSession.QuizCategory.FeatureType);
        QuizCardAbstract card = await quizCardService.GetCardForSession(sessionid, cardIndex);
        return MapQuizCardDto(card);
    }

    public async Task<QuizCardDtoAbstract> CreateNextCardForSession(ClaimsPrincipal claimsPrincipal, Guid sessionid)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        if(gameSession.IsFinished)
        {
            throw HandledExceptionFactory.Create("Game session is already finished");
        }

        // TODO: add check if the user has not yet answered the current card throw exception

        IQuizCardService quizCardService = GetQuizCardService(gameSession.QuizCategory.FeatureType);
        QuizCardAbstract card = await quizCardService.CreateCardForSession(gameSession);

        if (gameSession.CurrentCardIndex == gameSession.CardsCount - 1)
        {
            gameSession.IsFinished = true;
        }
        gameSession.CurrentCardIndex++;

        await _singleGameSessionRepository.Update(gameSession).ConfigureAwait(false);

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
            QuizCardFloat quizCardFloat => new QuizCardFloatDto(quizCardFloat.Id, quizCardFloat.CardIndex, quizCardFloat.Option1.Value.Value, quizCardFloat.Option2.Value.Value, quizCardFloat.OptionPicked ?? -1),
            //QuizCardInt quizCardInt => _mapper.Map<QuizCardIntDto>(quizCardInt),
            QuizCardInt quizCardInt => new QuizCardIntDto(quizCardInt.Id, quizCardInt.CardIndex, quizCardInt.Option1.Value.Value, quizCardInt.Option2.Value.Value, quizCardInt.OptionPicked ?? -1),
            _ => throw CriticalExceptionFactory.Create($"Unrecognised {nameof(QuizCardAbstract)}: {quizCard}")
        };
    }
}
