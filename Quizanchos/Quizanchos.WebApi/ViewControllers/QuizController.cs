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

        QuizOptionViewModel[] options = await GetQuizOptionViewModel(quizCardDto.EntitiesId);

        var viewModel = new QuizViewModel
        {
            CurrentCardIndex = singleGameSessionDto.CurrentCardIndex + 1 ,
            TotalCards = (int)singleGameSessionDto.CardsCount,
            SessionId = singleGameSessionDto.Id,
            CreationTime = quizCardDto.CreationTime,
            SecondsPerCard = (int)singleGameSessionDto.SecondPerCard,
            OptionCount = (int)singleGameSessionDto.OptionCount,
            Score = singleGameSessionDto.Score,
            CategoryId = singleGameSessionDto.QuizCategoryId,
            QuizCategoryName = await GetQuizCategoryName(singleGameSessionDto.QuizCategoryId),
            Options = options
        };
        
        return View(viewModel);
    }

    private async Task<QuizOptionViewModel[]> GetQuizOptionViewModel(Guid[] entitiesId)
    {
        List<QuizOptionViewModel> options = new();

        foreach(Guid id in entitiesId)
        {
            var entity = await _QuizEntityService.GetById(id);
            options.Add(new QuizOptionViewModel
            {
                Id = entity.Id,
                Name = entity.Name
            });
        }

        return options.ToArray();
    }

    private async Task<string> GetQuizCategoryName(Guid quizCategoryId)
    {
        var quizCategory = await _quizCategoryService.GetById(quizCategoryId);
        return quizCategory?.Name ?? "Unknown Category";
    }
}

