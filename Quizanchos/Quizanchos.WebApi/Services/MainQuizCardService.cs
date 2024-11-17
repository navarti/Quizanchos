using AutoMapper;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
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
        
        return new QuizCardFloatDto(Guid.NewGuid(), 0, 0, 0, 0);
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
}
