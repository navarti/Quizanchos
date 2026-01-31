using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Quizanchos.WebApi.Controllers;

[Route("Quiz")]
public class QuizViewController : Controller
{
    [HttpGet("Setup/{categoryId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Setup(Guid categoryId)
    {
        // Pass only the category ID to the view
        // The view will load all data via API
        ViewBag.CategoryId = categoryId;
        return View("~/Views/Quiz/SessionSetup.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    public IActionResult Game(Guid gameId)
    {
        // Pass only the game ID to the view
        // The view will load all data via API
        ViewBag.GameId = gameId;
        return View("~/Views/Quiz/Game.cshtml");
    }
}
