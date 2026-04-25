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
        [Authorize(AppRole.Admin)]
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
        [Authorize(AppRole.Admin)]
        public IActionResult Admin()
        {
            return View();
        }

        [HttpGet("/Admin/Users")]
        [Authorize(AppRole.Admin)]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet("/Admin/CreateQuiz")]
        [Authorize(AppRole.Admin)]
        public IActionResult CreateQuiz()
        {
            return View();
        }

        [HttpGet("/Admin/TopUp")]
        [Authorize(AppRole.Admin)]
        public IActionResult TopUp()
        {
            return View();
        }
    }
}
