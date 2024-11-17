using AutoMapper;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.HelperServices;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class SingleGameSessionService
{
    private readonly IMapper _mapper;
    private readonly ISingleGameSessionRepository _singleGameSessionRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly UserRetrieverService _userRetrieverService;

    public SingleGameSessionService(
        IMapper mapper, 
        ISingleGameSessionRepository singleGameSessionRepository, 
        IQuizCategoryRepository quizCategoryRepository,
        UserRetrieverService userRetrieverService)
    {
        _mapper = mapper;
        _singleGameSessionRepository = singleGameSessionRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _userRetrieverService = userRetrieverService;
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
            Score = 0,
            CurrentCardIndex = 0,
            CardsCount = 10,
            GameLevel = baseSingleGameSessionDto.GameLevel
        };

        gameSession = await _singleGameSessionRepository.Create(gameSession).ConfigureAwait(false);

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto> GetById(Guid id)
    {
        SingleGameSession gameSession = await _singleGameSessionRepository.GetById(id).ConfigureAwait(false);
        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }

    public async Task<SingleGameSessionDto?> FindAliveSession(ClaimsPrincipal claimsPrincipal)
    {
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        SingleGameSession? gameSession = await _singleGameSessionRepository.FindAliveGameSessionForUserIncluding(userId).ConfigureAwait(false);
        if(gameSession is null)
        {
            return null;
        }

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }
}
