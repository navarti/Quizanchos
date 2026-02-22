using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Quizanchos.WebApi.Controllers.QuizMultiplayer;

[Route("QuizMultiplayer")]
public class QuizMultiplayerViewController : Controller
{
    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Lobby()
    {
        return View("~/Views/QuizMultiplayer/Lobby.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Game(Guid gameId)
    {
        ViewBag.GameId = gameId;
        return View("~/Views/QuizMultiplayer/Game.cshtml");
    }
}
