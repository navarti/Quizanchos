using AutoMapper;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Services.HelperServices;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class SingleGameSessionService
{
    private readonly IMapper _mapper;
    private readonly ISingleGameSessionRepository _singleGameSessionRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly MainQuizCardService _mainQuizCardService;
    private readonly SessionTerminatorService _sessionTerminatorService;

    public SingleGameSessionService(
        IMapper mapper,
        ISingleGameSessionRepository singleGameSessionRepository,
        IQuizCategoryRepository quizCategoryRepository,
        UserRetrieverService userRetrieverService,
        MainQuizCardService mainQuizCardService,
        SessionTerminatorService sessionTerminatorService)
    {
        _mapper = mapper;
        _singleGameSessionRepository = singleGameSessionRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _userRetrieverService = userRetrieverService;
        _mainQuizCardService = mainQuizCardService;
        _sessionTerminatorService = sessionTerminatorService;
    }

    public async Task<SingleGameSessionDto> Create(BaseSingleGameSessionDto baseSingleGameSessionDto, ClaimsPrincipal claimsPrincipal)
    {
        _ = baseSingleGameSessionDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseSingleGameSessionDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(baseSingleGameSessionDto.QuizCategoryId).ConfigureAwait(false);

        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        SingleGameSession? existingGameSession = await _singleGameSessionRepository.FindAliveGameSessionForUser(user.Id).ConfigureAwait(false);
        if (existingGameSession is not null)
        {
            throw HandledExceptionFactory.Create("There is already an active game session for this user.");
        }

        SingleGameSession gameSession = new()
        {
            QuizCategory = quizCategory,
            ApplicationUser = user,
            CreationTime = DateTime.UtcNow,
            IsFinished = false,
            IsTerminatedByTime = false,
            Score = 0,
            CurrentCardIndex = -1,
            CardsCount = 5,
            GameLevel = baseSingleGameSessionDto.GameLevel,
            SecondsPerCard = 10
        };

        gameSession = await _singleGameSessionRepository.Create(gameSession).ConfigureAwait(false);

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto> GetById(Guid id)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetById(id).ConfigureAwait(false);
        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);
        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto?> FindAliveSession(ClaimsPrincipal claimsPrincipal)
    {
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        SingleGameSession? gameSession = await _singleGameSessionRepository.FindAliveGameSessionForUserIncluding(userId).ConfigureAwait(false);
        if (gameSession is null)
        {
            return null;
        }
        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<QuizCardDtoAbstract> GetCardForSession(ClaimsPrincipal claimsPrincipal, Guid sessionid, int cardIndex)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        return await _mainQuizCardService.GetCardDtoForSession(gameSession, cardIndex);
    }

    public async Task<QuizCardDtoAbstract> CreateNextCardForSession(ClaimsPrincipal claimsPrincipal, Guid sessionid)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);
        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        if (gameSession.IsFinished)
        {
            throw HandledExceptionFactory.Create("Game session is already finished");
        }

        gameSession.CurrentCardIndex++;
        await _singleGameSessionRepository.Update(gameSession).ConfigureAwait(false);
        if (gameSession.CurrentCardIndex == gameSession.CardsCount - 1)
        {
            gameSession.IsFinished = true;
        }

        return await _mainQuizCardService.CreateNextCardForSession(gameSession).ConfigureAwait(false);
    }

    public async Task PickAnswer(ClaimsPrincipal claimsPrincipal, Guid sessionid)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);
        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        // make dto (session + answer)
    }
}
