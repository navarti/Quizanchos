using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Quiz.Services;
using Quizanchos.ViewModels.Admin;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.Payment;

namespace Quizanchos.WebApi.ViewControllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuizCategoryService _quizCategoryService;
    private readonly TopUpService _topUpService;

    public AdminController(
        ILogger<AdminController> logger,
        UserManager<ApplicationUser> userManager,
        QuizCategoryService quizCategoryService,
        TopUpService topUpService)
    {
        _logger = logger;
        _userManager = userManager;
        _quizCategoryService = quizCategoryService;
        _topUpService = topUpService;
    }

    [HttpGet("/Admin/Create")]
    [Authorize(AppRole.Admin)]
    public IActionResult Signup() => View();

    [HttpGet("/Admin/Signin")]
    [AllowAnonymous]
    public IActionResult Signin() => View();

    [HttpGet("/Admin")]
    [Authorize(AppRole.Admin)]
    public async Task<IActionResult> Admin()
    {
        var viewModel = await BuildDashboardViewModelAsync().ConfigureAwait(false);
        return View(viewModel);
    }

    [HttpGet("/Admin/Users")]
    [Authorize(AppRole.Admin)]
    public IActionResult Users() => View();

    [HttpGet("/Admin/CreateQuiz")]
    [Authorize(AppRole.Admin)]
    public IActionResult CreateQuiz() => View();

    [HttpGet("/Admin/TopUp")]
    [Authorize(AppRole.Admin)]
    public IActionResult TopUp() => View();

    /// <summary>
    /// Aggregates dashboard counts. Each metric is loaded independently so a
    /// partial failure (e.g. payments service down) still yields a usable
    /// dashboard with the rest of the data.
    /// </summary>
    private async Task<AdminDashboardViewModel> BuildDashboardViewModelAsync()
    {
        var stats = new List<AdminStatCardViewModel>();
        string? loadError = null;

        // Total registered users
        try
        {
            int totalUsers = await _userManager.Users.CountAsync().ConfigureAwait(false);
            stats.Add(new AdminStatCardViewModel
            {
                Key = "users",
                Title = "Total Users",
                Value = totalUsers.ToString("N0"),
                Caption = "Registered users on the platform.",
                IconKey = "users",
                Accent = "primary",
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load user count for admin dashboard.");
            stats.Add(UnavailableStat("users", "Total Users", "Registered users on the platform.", "users"));
            loadError ??= "Some metrics could not be loaded. Refresh the page to retry.";
        }

        // Active quiz categories
        try
        {
            var categories = await _quizCategoryService.GetAll().ConfigureAwait(false);
            stats.Add(new AdminStatCardViewModel
            {
                Key = "categories",
                Title = "Quiz Categories",
                Value = categories.Count.ToString("N0"),
                Caption = "Published quiz categories.",
                IconKey = "content",
                Accent = "primary",
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load quiz category count for admin dashboard.");
            stats.Add(UnavailableStat("categories", "Quiz Categories", "Published quiz categories.", "content"));
            loadError ??= "Some metrics could not be loaded. Refresh the page to retry.";
        }

        // Pending top-ups
        try
        {
            var pending = await _topUpService.GetAllPendingOrdersAsync().ConfigureAwait(false);
            stats.Add(new AdminStatCardViewModel
            {
                Key = "pending-topups",
                Title = "Pending Top-Ups",
                Value = pending.Count.ToString("N0"),
                Caption = "Awaiting confirmation.",
                IconKey = "coins",
                Accent = pending.Count > 0 ? "warning" : "primary",
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load pending top-up count for admin dashboard.");
            stats.Add(UnavailableStat("pending-topups", "Pending Top-Ups", "Awaiting confirmation.", "coins"));
            loadError ??= "Some metrics could not be loaded. Refresh the page to retry.";
        }

        var tools = new List<AdminToolCardViewModel>
        {
            new()
            {
                Key = "users",
                Title = "User Management",
                Description = "Search, review and remove platform users.",
                IconKey = "users",
                ActionLabel = "View Users",
                OpenModalId = "userManagementModal",
            },
            new()
            {
                Key = "content",
                Title = "Content Management",
                Description = "Manage quiz categories and their entities.",
                IconKey = "content",
                ActionLabel = "Manage Content",
                OpenModalId = "categoryManagementModal",
            },
            new()
            {
                Key = "create-quiz",
                Title = "Create Quiz",
                Description = "Build a new quiz category, entities and features.",
                IconKey = "plus",
                ActionLabel = "Open Editor",
                ActionUrl = "/Admin/CreateQuiz",
            },
            new()
            {
                Key = "topup",
                Title = "Top-Up Orders",
                Description = "Review and confirm crypto top-up payments.",
                IconKey = "coins",
                ActionLabel = "View Orders",
                ActionUrl = "/Admin/TopUp",
            },
            new()
            {
                Key = "logs",
                Title = "System Logs",
                Description = "View recent activity and errors.",
                IconKey = "logs",
                ActionLabel = "View Logs",
                IsEnabled = false,
                DisabledReason = "Logs viewer is not yet wired up.",
            },
            new()
            {
                Key = "settings",
                Title = "Admin Settings",
                Description = "Configure system and permissions.",
                IconKey = "settings",
                ActionLabel = "Go to Settings",
                IsEnabled = false,
                DisabledReason = "Settings page is not yet implemented.",
            },
        };

        return new AdminDashboardViewModel
        {
            Stats = stats,
            Tools = tools,
            LoadError = loadError,
        };
    }

    private static AdminStatCardViewModel UnavailableStat(string key, string title, string caption, string iconKey) => new()
    {
        Key = key,
        Title = title,
        Value = "—",
        Caption = caption,
        IconKey = iconKey,
        IsAvailable = false,
        UnavailableReason = "Could not load this metric.",
    };
}
