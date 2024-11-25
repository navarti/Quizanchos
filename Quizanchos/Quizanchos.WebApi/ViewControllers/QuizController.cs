using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;
using Quizanchos.ViewModels;
namespace Quizanchos.WebApi.ViewControllers;

[Route("Quiz")]
public class QuizController : Controller
{
    private readonly SingleGameSessionService _singleGameSessionService;
    private readonly ILogger<QuizController> _logger;
    private readonly QuizCategoryService _quizCategoryService;

    public QuizController(SingleGameSessionService singleGameSessionService, ILogger<QuizController>? logger, QuizCategoryService quizCategoryService)
    {
        _singleGameSessionService = singleGameSessionService;
        _logger = logger;
        _quizCategoryService = quizCategoryService ?? throw new ArgumentNullException(nameof(quizCategoryService));
    }
    [HttpGet("Setup/{quizcategoryid:guid}")]
    public async Task<IActionResult> SessionSetup(Guid quizcategoryid)
    {
        var viewModel = new QuizCategoryViewModel()
        {
            QuizCategoryName = await GetQuizCategoryName(quizcategoryid)
        };
        return View(viewModel);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> SingleGameSession(Guid id)
    {

        
        SingleGameSessionDto singleGameSessionDto = await _singleGameSessionService.GetById(id);
        
        var viewModel = new SingleGameSessionViewModel
        {
            Id = singleGameSessionDto.Id,
            UserId = singleGameSessionDto.UserId,
            CreationTime = singleGameSessionDto.CreationTime,
            CurrentCardIndex = singleGameSessionDto.CurrentCardIndex,
            Score = singleGameSessionDto.Score,
            IsFinished = singleGameSessionDto.IsFinished,
            IsTerminatedByTime = singleGameSessionDto.IsTerminatedByTime,
            CardsCount = singleGameSessionDto.CardsCount,
            SecondsPerCard = singleGameSessionDto.SecondPerCard,
            QuizCategoryId = singleGameSessionDto.QuizCategoryId,
            QuizCategoryName = await GetQuizCategoryName(singleGameSessionDto.QuizCategoryId), 
            GameLevel = singleGameSessionDto.GameLevel.ToString() 
        };
        return View(viewModel);

    }
    private async Task<string> GetQuizCategoryName(Guid quizCategoryId)
    {
        var quizCategory = await _quizCategoryService.GetById(quizCategoryId);
        return quizCategory?.Name ?? "Unknown Category";
    }
}

