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
            QuizCategoryName = await GetQuizCategoryName(quizcategoryid),
            CategoryId = quizcategoryid
        };
        return View(viewModel);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> SingleGameSession(Guid id)
    {
        SingleGameSessionDto singleGameSessionDto = await _singleGameSessionService.GetById(User,id);
        
       return View();

    }
    private async Task<string> GetQuizCategoryName(Guid quizCategoryId)
    {
        var quizCategory = await _quizCategoryService.GetById(quizCategoryId);
        return quizCategory?.Name ?? "Unknown Category";
    }
}

