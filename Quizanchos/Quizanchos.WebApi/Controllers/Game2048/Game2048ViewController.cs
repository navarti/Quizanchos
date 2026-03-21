using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Controllers.Game2048;

[Route("Game2048")]
public class Game2048ViewController : Controller
{
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;

    public Game2048ViewController(IMinigameFrontendRegistry minigameFrontendRegistry)
    {
        _minigameFrontendRegistry = minigameFrontendRegistry;
    }

    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Index()
    {
        ViewBag.MinigameTypeId = _minigameFrontendRegistry.GetDescriptor("Game2048")?.MinigameTypeId ?? 2;
        return View("~/Views/Game2048/Index.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Game(Guid gameId)
    {
        ViewBag.GameId = gameId;
        ViewBag.MinigameTypeId = _minigameFrontendRegistry.GetDescriptor("Game2048")?.MinigameTypeId ?? 2;
        return View("~/Views/Game2048/Game.cshtml");
    }
}
