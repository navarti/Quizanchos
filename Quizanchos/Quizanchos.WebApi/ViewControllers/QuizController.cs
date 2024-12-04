using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;
using Quizanchos.ViewModels;
using Quizanchos.WebApi.Dto.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Quizanchos.WebApi.ViewControllers;

[Route("Quiz")]
public class QuizController : Controller
{
    private readonly SingleGameSessionService _singleGameSessionService;
    private readonly ILogger<QuizController> _logger;
    private readonly QuizCategoryService _quizCategoryService;
    private readonly QuizEntityService _QuizEntityService;

    public QuizController(SingleGameSessionService singleGameSessionService, ILogger<QuizController>? logger, QuizCategoryService quizCategoryService,QuizEntityService quizEntityService)
    {
        _singleGameSessionService = singleGameSessionService;
        _logger = logger;
        _quizCategoryService = quizCategoryService ?? throw new ArgumentNullException(nameof(quizCategoryService));
        _QuizEntityService = quizEntityService;
    }

    [HttpGet("Setup/{quizcategoryid:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SessionSetup(Guid quizcategoryid)
    {
        var viewModel = new QuizCategoryViewModel()
        {
            QuizCategoryName = await GetQuizCategoryName(quizcategoryid),
            CategoryId = quizcategoryid
        };
        return View(viewModel);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> SingleGameSession(Guid sessionId)
    {
        SingleGameSessionDto singleGameSessionDto = await _singleGameSessionService.GetById(User,sessionId);
        
        QuizCardDtoAbstract quizCardDto = await _singleGameSessionService.GetCurrentCardForSession(User, sessionId);

        var options = new List<QuizOptionViewModel>();
        for (int i = 1; i <= (int)singleGameSessionDto.OptionCount; i++)
        {
            var propertyName = $"Entity{i}Id";
            var propertyInfo = quizCardDto.GetType().GetProperty(propertyName);

            if (propertyInfo != null)
            {
                var entityId = propertyInfo.GetValue(quizCardDto) as Guid?;
                if (entityId.HasValue)
                {
                    var entity = await _QuizEntityService.GetById(entityId.Value);
                    options.Add(new QuizOptionViewModel
                    {
                        Id = entity.Id,
                        Name = entity.Name
                    });
                }
            }
        }
        
        var viewModel = new QuizViewModel
        {
            CurrentCardIndex = singleGameSessionDto.CurrentCardIndex + 1 ,
            TotalCards = (int)singleGameSessionDto.CardsCount,
            SessionId = singleGameSessionDto.Id,
            CreationTime = quizCardDto.CreationTime,
            SecondsPerCard = (int)singleGameSessionDto.SecondPerCard,
            OptionCount = options.Count,
            Score = singleGameSessionDto.Score,
            CategoryId = singleGameSessionDto.QuizCategoryId,
            QuizCategoryName = await GetQuizCategoryName(singleGameSessionDto.QuizCategoryId),
            Options = options
        };

        
       return View(viewModel);

    }
    private async Task<string> GetQuizCategoryName(Guid quizCategoryId)
    {
        var quizCategory = await _quizCategoryService.GetById(quizCategoryId);
        return quizCategory?.Name ?? "Unknown Category";
    }
}

