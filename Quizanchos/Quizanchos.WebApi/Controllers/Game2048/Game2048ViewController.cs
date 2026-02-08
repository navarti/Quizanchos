using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Quizanchos.WebApi.Controllers.Game2048;

[Route("Game2048")]
public class Game2048ViewController : Controller
{
    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Index()
    {
        return View("~/Views/Game2048/Index.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Game(Guid gameId)
    {
        ViewBag.GameId = gameId;
        return View("~/Views/Game2048/Game.cshtml");
    }
}
