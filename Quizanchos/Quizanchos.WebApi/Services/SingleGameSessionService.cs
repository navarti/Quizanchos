using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Util;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class SingleGameSessionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ISingleGameSessionRepository _singleGameSessionRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;

    public SingleGameSessionService(
        UserManager<ApplicationUser> userManager, 
        IMapper mapper, 
        ISingleGameSessionRepository singleGameSessionRepository, 
        IQuizCategoryRepository quizCategoryRepository)
    {
        _userManager = userManager;
        _mapper = mapper;
        _singleGameSessionRepository = singleGameSessionRepository;
        _quizCategoryRepository = quizCategoryRepository;
    }

    public async Task<SingleGameSessionDto> Create(BaseSingleGameSessionDto baseSingleGameSessionDto, ClaimsPrincipal claimsPrincipal)
    {
        _ = baseSingleGameSessionDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseSingleGameSessionDto));

        QuizCategory? quizCategory = await _quizCategoryRepository.FindById(baseSingleGameSessionDto.QuizCategoryId).ConfigureAwait(false);
        _ = quizCategory ?? throw HandledExceptionFactory.CreateIdNotFoundException(baseSingleGameSessionDto.QuizCategoryId, nameof(quizCategory));

        string? userId = _userManager.GetUserId(claimsPrincipal);
        _ = userId ?? throw HandledExceptionFactory.CreateNullException(nameof(userId));

        ApplicationUser? user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        _ = user ?? throw HandledExceptionFactory.CreateIdNotFoundException(userId, nameof(user));

        SingleGameSession gameSession = new()
        {
            QuizCategory = quizCategory,
            ApplicationUser = user,
            CreationTime = DateTime.UtcNow,
        };

        gameSession = await _singleGameSessionRepository.Create(gameSession).ConfigureAwait(false);

        return _mapper.Map<SingleGameSessionDto>(gameSession);
    }
}
