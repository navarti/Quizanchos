using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Implementations;
using Quizanchos.Quiz.Dto;
using Quizanchos.Quiz.Services;
using Quizanchos.ViewModels;
using Quizanchos.WebApi.Services;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.Users;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Quizanchos.WebApi.ViewControllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    
    private readonly QuizCategoryService _quizCategoryService;
    private readonly LeaderBoardService _leaderBoardService;
    private readonly GameService _gameService;
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;
    private readonly UserProfileService _userProfileService;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public HomeController(
        LeaderBoardService leaderBoardService,
        ILogger<HomeController> logger,
        QuizCategoryService quizCategoryService,
        GameService gameService,
        IMinigameFrontendRegistry minigameFrontendRegistry,
        UserProfileService userProfileService,
        UserManager<ApplicationUser> userManager)
    {
        _leaderBoardService = leaderBoardService;
        _logger = logger;
        _quizCategoryService = quizCategoryService;
        _gameService = gameService;
        _minigameFrontendRegistry = minigameFrontendRegistry;
        _userProfileService = userProfileService;
        _userManager = userManager;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        List<QuizCategoryDto> quizCategories = await _quizCategoryService.GetAll();
        IGameState? activeGameSession = null;
        string quizName = "Unknown Category";
        int activeUsersCount = await _userManager.Users.CountAsync().ConfigureAwait(false);
        string? activeSessionMinigameName = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
                activeGameSession = (await _gameService.GetActiveGameByPlayerIdAsync(userId)).Response?.State;
        }

        var minigames = GetMinigameCards();

        string? activeSessionUrl = null;
        if (activeGameSession != null)
        {
            var descriptor = _minigameFrontendRegistry.GetDescriptor(activeGameSession.MinigameType);
            if (descriptor != null)
            {
                activeSessionMinigameName = descriptor.DisplayName;
                activeSessionUrl = descriptor.GameUrlTemplate.Replace("{gameId}", activeGameSession.GameId.ToString());
            }
        }

        var viewModel = new HomeViewModel
        {
            QuizCategories = quizCategories,
            ActiveSession = activeGameSession,
            ActiveSessionMinigameName = activeSessionMinigameName,
            ActiveSessionUrl = activeSessionUrl,
            QuizName = quizName,
            Minigames = minigames,
            ActiveUsersCount = activeUsersCount,
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
        var minigames = GetMinigameCards();

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
            Minigames = minigames,
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

    [HttpGet("/Minigames")]
    public IActionResult Minigames()
    {
        var viewModel = new HomeViewModel
        {
            QuizCategories = new List<QuizCategoryDto>(),
            QuizName = string.Empty,
            Minigames = GetMinigameCards(),
            CurrentUserName = User.Identity?.Name ?? "Guest"
        };

        return View(viewModel);
    }

    [HttpGet("/Market")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> Market()
    {
        var userDto = await _userProfileService.GetUserInfo(User);
        return View(userDto);
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

    private List<MinigameCardViewModel> GetMinigameCards()
    {
        return _minigameFrontendRegistry
            .GetAllDescriptors()
            .Values
            .OrderBy(x => x.Order)
            .Select(x => new MinigameCardViewModel
            {
                MinigameTypeId = x.MinigameTypeId,
                GameKey = x.GameKey,
                DisplayName = x.DisplayName,
                Description = x.Description,
                CardStyle = x.CardStyle,
                LobbyUrl = x.LobbyUrl,
                GameUrlTemplate = x.GameUrlTemplate,
                ActionText = x.ActionText,
                Order = x.Order
            })
            .ToList();
    }
    
}
