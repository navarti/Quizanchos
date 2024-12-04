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
    
    public HomeController(ILogger<HomeController> logger, QuizCategoryService quizCategoryService)
    {
        _logger = logger;
        _quizCategoryService = quizCategoryService ?? throw new ArgumentNullException(nameof(quizCategoryService));
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        List<QuizCategoryDto> quizCategories = await _quizCategoryService.GetAll();
        var viewModel = new HomeViewModel
        {
            QuizCategories = quizCategories
        };
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
