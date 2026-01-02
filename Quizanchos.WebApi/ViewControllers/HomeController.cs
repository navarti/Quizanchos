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

        var users = await _leaderBoardService.GetLeaderBoardAsync(take: 3, skip: 0); 
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
            QuizName = quizName,
            Users = users.ToList()
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
        var users = await _leaderBoardService.GetLeaderBoardAsync(take: 10, skip: 0); 
        var currentUser = users.FirstOrDefault(u => u.UserName == User.Identity.Name);
        var currentUserName = User.Identity?.Name ?? "Guest"; 
        int itemsPerPage = 10;
        int totalUsers = users.Count;
        int totalPages = (int)Math.Ceiling(totalUsers / (double)itemsPerPage);
        var currentPageUsers = users
            .OrderBy(u => u.Position)
            .Take(itemsPerPage - 1)
            .ToList();
        if (currentUser != null && !currentPageUsers.Contains(currentUser))
        {
            currentPageUsers.Insert(0, currentUser);
            if (currentPageUsers.Count > itemsPerPage)
            {
                currentPageUsers = currentPageUsers.Take(itemsPerPage).ToList();
            }
        }
        var viewModel = new HomeViewModel
        {
            Users = users.ToList(),
            CurrentUserName = currentUserName,
            TotalPages = totalPages,
            CurrentPage = 1
        };

        return View(viewModel);

    }

    
    [HttpGet("/FAQ")]
    public IActionResult Faq()
    {
        return View();
    }
    
    [HttpGet("/Contact")]
    public IActionResult Contact()
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
    
}
