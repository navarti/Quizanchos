using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Quizanchos.ViewModels;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
namespace Quizanchos.WebApi.ViewControllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    
    private readonly QuizCategoryService _quizCategoryService;
    private readonly SingleGameSessionService _singleGameSessionService;
    private readonly LeaderBoardService _leaderBoardService;
    
    public HomeController(SingleGameSessionService singleGameSessionService,LeaderBoardService leaderBoardService, ILogger<HomeController> logger, QuizCategoryService quizCategoryService)
    {
        _singleGameSessionService = singleGameSessionService;
        _leaderBoardService = leaderBoardService;
        _logger = logger;
        _quizCategoryService = quizCategoryService ?? throw new ArgumentNullException(nameof(quizCategoryService));
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        List<QuizCategoryDto> quizCategories = await _quizCategoryService.GetAll();
        SingleGameSessionDto? singleGameSession = null;
        string quizName = "Unknown Category";

        // Проверяем, авторизован ли пользователь
        if (User.Identity?.IsAuthenticated == true)
        {
            singleGameSession = await _singleGameSessionService.FindAliveSession(User);

            if (singleGameSession != null)
            {
                var quizCategory = await _quizCategoryService.GetById(singleGameSession.QuizCategoryId);
                quizName = quizCategory?.Name + " Quiz" ?? "Unknown Category";
            }
        }
        var viewModel = new HomeViewModel
        {
            QuizCategories = quizCategories,
            ActiveSession = singleGameSession,
            QuizName = quizName
        };
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [HttpGet("/Leaderboard")]
    public async Task<IActionResult> Leaderboard()
    {
        var leaderBoardResult = await _leaderBoardService.GetLeaderBoardAsync(take: 10, skip: 0); 
        var users = leaderBoardResult.Users.ToList(); 
        var currentUserName = User.Identity?.Name ?? "Guest"; 

        var viewModel = new HomeViewModel
        {
            Users = users,
            CurrentUserName = currentUserName
        };

        return View(viewModel);

    }

    
    [HttpGet("/FAQ")]
    public IActionResult Faq()
    {
        return View();
    }
    
    [HttpGet("/Signup")]
    public IActionResult Signup()
    {
        return View();
    }
    [HttpGet("/Signin")]
    public IActionResult Signin()
    {
        return View();
    }
    
    [HttpGet("/QuizCategories")]
    public async Task<IActionResult> QuizCategories()
    {
        List<QuizCategoryDto> quizCategories = await _quizCategoryService.GetAll();
        var viewModel = new HomeViewModel
        {
            QuizCategories = quizCategories
        };
        return View(viewModel);
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        
        return RedirectToAction("Index", "Home");
    }
    
}
