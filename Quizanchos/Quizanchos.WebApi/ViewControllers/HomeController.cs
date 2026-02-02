using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Quizanchos.ViewModels;
using Quizanchos.Quiz.Dto;
using Quizanchos.Quiz.Services;
using Quizanchos.WebApi.Services.Users;
using Quizanchos.Domain.Repositories.Implementations;

namespace Quizanchos.WebApi.ViewControllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    
    private readonly QuizCategoryService _quizCategoryService;
    private readonly LeaderBoardService _leaderBoardService;
    private readonly GameSessionRepository _gameSessionRepository;
    
    public HomeController(LeaderBoardService leaderBoardService, ILogger<HomeController> logger, QuizCategoryService quizCategoryService, GameSessionRepository gameSessionRepository)
    {
        _leaderBoardService = leaderBoardService;
        _logger = logger;
        _quizCategoryService = quizCategoryService;
        _gameSessionRepository = gameSessionRepository;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        List<QuizCategoryDto> quizCategories = await _quizCategoryService.GetAll();
        object? activeGameSession = null;
        string quizName = "Unknown Category";

        var users = await _leaderBoardService.GetLeaderBoardAsync(take: 3, skip: 0); 
        if (User.Identity?.IsAuthenticated == true)
        {
            // TODO:GameSession
            // activeGameSession = ...
        }

        var viewModel = new HomeViewModel
        {
            QuizCategories = quizCategories,
            ActiveSession = activeGameSession,
            QuizName = quizName,
            Users = users.ToList(),
            CurrentUserName = User.Identity?.Name ?? "Guest"
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
        var currentUserName = User.Identity?.Name ?? "Guest";
        var currentUser = users.FirstOrDefault(u => u.UserName == currentUserName);
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
            QuizCategories = new List<QuizCategoryDto>(),
            QuizName = string.Empty,
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
            QuizCategories = quizCategories,
            QuizName = string.Empty,
            CurrentUserName = User.Identity?.Name ?? "Guest"
        };
        return View(viewModel);
    }
    
}
