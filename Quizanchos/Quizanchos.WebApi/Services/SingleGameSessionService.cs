using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public SingleGameSessionService(
        IMapper mapper,
        ISingleGameSessionRepository singleGameSessionRepository,
        IQuizCategoryRepository quizCategoryRepository,
        UserRetrieverService userRetrieverService,
        MainQuizCardService mainQuizCardService,
        SessionTerminatorService sessionTerminatorService,
        UserManager<ApplicationUser> userManager)
    {
        _mapper = mapper;
        _singleGameSessionRepository = singleGameSessionRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _userRetrieverService = userRetrieverService;
        _mainQuizCardService = mainQuizCardService;
        _sessionTerminatorService = sessionTerminatorService;
        _userManager = userManager;
    }

    public async Task<SingleGameSessionDto> Create(BaseSingleGameSessionDto baseSingleGameSessionDto, ClaimsPrincipal claimsPrincipal)
    {
        _ = baseSingleGameSessionDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseSingleGameSessionDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(baseSingleGameSessionDto.QuizCategoryId).ConfigureAwait(false);

        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        SingleGameSession? existingGameSession = await FindAliveSession(user.Id).ConfigureAwait(false);
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
            CardsCount = baseSingleGameSessionDto.CardsCount,
            GameLevel = baseSingleGameSessionDto.GameLevel,
            SecondsPerCard = baseSingleGameSessionDto.SecondPerCard,
            OptionCount = baseSingleGameSessionDto.OptionCount
        };

        gameSession = await _singleGameSessionRepository.Create(gameSession).ConfigureAwait(false);

        await CreateNextCardForSession(gameSession).ConfigureAwait(false);

        gameSession = await _singleGameSessionRepository.Update(gameSession).ConfigureAwait(false);

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto> GetById(ClaimsPrincipal claimsPrincipal, Guid id)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(id).ConfigureAwait(false);
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);
        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto?> FindAliveSession(ClaimsPrincipal claimsPrincipal)
    {
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        SingleGameSession? aliveSession = await FindAliveSession(userId).ConfigureAwait(false);
        if(aliveSession is null)
        {
            return null;
        }
        return _mapper.Map<SingleGameSessionDto>(aliveSession);
    }

    public async Task<QuizCardDtoAbstract> GetCurrentCardForSession(ClaimsPrincipal claimsPrincipal, Guid sessionid)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(sessionid).ConfigureAwait(false);

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        if (gameSession.ApplicationUser.Id != userId)
        {
            throw HandledExceptionFactory.CreateForbiddenException();
        }

        return await _mainQuizCardService.GetCardDtoForSession(gameSession, gameSession.CurrentCardIndex);
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

        QuizCardDtoAbstract cardCreated = await CreateNextCardForSession(gameSession).ConfigureAwait(false);

        await _singleGameSessionRepository.Update(gameSession).ConfigureAwait(false);

        return cardCreated;
    }

    public async Task<QuizCardDtoAbstract> PickAnswerForSession(ClaimsPrincipal claimsPrincipal, AnswerDto answerDto)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetByIdIncluding(answerDto.Sessionid).ConfigureAwait(false);
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

        if(answerDto.OptionPicked < 0 || answerDto.OptionPicked > (int)gameSession.OptionCount - 1)
        {
            throw HandledExceptionFactory.Create("Invalid option picked");
        }

        if (gameSession.CurrentCardIndex == (int)gameSession.CardsCount - 1)
        {
            gameSession.IsFinished = true;

            await _userManager.FindByIdAsync(userId).ContinueWith(async task =>
            {
                ApplicationUser user = task.Result;
                user.Score += gameSession.Score;
                await _userManager.UpdateAsync(user);
            });
        }
        await _singleGameSessionRepository.Update(gameSession).ConfigureAwait(false);

        (QuizCardDtoAbstract QuizCard, bool IsCorrect) result = await _mainQuizCardService.PickAnswerForSession(gameSession, answerDto.OptionPicked);

        if(result.IsCorrect)
        {
            gameSession.Score++;
        }
        await _singleGameSessionRepository.Update(gameSession);

        return result.QuizCard;
    }

    public async Task<SingleGameSession?> FindAliveSession(string userId)
    {
        SingleGameSession? gameSession = await _singleGameSessionRepository.FindAliveGameSessionForUserIncluding(userId).ConfigureAwait(false);
        if (gameSession is null)
        {
            return null;
        }
        await _sessionTerminatorService.TerminateSessionIfNeeded(gameSession);
        if (gameSession.IsFinished)
        {
            return null;
        }

        return gameSession;
    }

    private async Task<QuizCardDtoAbstract> CreateNextCardForSession(SingleGameSession gameSession)
    {
        if (gameSession.IsFinished)
        {
            throw HandledExceptionFactory.Create("Game session is already finished");
        }

        if (gameSession.CurrentCardIndex != -1)
        {
            QuizCardAbstract currentCard = await _mainQuizCardService.GetCardForSession(gameSession, gameSession.CurrentCardIndex);
            if (currentCard.OptionPicked is null)
            {
                throw HandledExceptionFactory.Create("You need to pick an option for the current card before requesting the next one.");
            }
        }

        gameSession.CurrentCardIndex++;

        return await _mainQuizCardService.CreateNextCardForSession(gameSession).ConfigureAwait(false);
    }   
}
