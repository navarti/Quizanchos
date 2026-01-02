using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Quizanchos.WebApi.Constants;

namespace Quizanchos.WebApi.ViewControllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }
        [HttpGet("/Admin/Create")]
        [Authorize(QuizRole.Admin)]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpGet("/Admin/Signin")]
        [AllowAnonymous]
        public IActionResult Signin()
        {
            return View();
        }

        [HttpGet("/Admin")]
        [Authorize(QuizRole.Admin)]
        public IActionResult Admin()
        {
            return View();
        }

        [HttpGet("/Admin/Users")]
        [Authorize(QuizRole.Admin)]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet("/Admin/CreateQuiz")]
        [Authorize(QuizRole.Admin)]
        public IActionResult CreateQuiz()
        {
            return View();
        }
    }
}
