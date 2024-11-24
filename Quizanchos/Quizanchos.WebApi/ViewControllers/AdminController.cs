using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Quizanchos.WebApi.ViewControllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }
        [HttpGet("/Admin/Signup")]
        [AllowAnonymous]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [HttpGet("/Admin/Users")]
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet("/Admin/Content")]
        [Authorize(Roles = "Admin")]
        public IActionResult Content()
        {
            return View();
        }
    }
}