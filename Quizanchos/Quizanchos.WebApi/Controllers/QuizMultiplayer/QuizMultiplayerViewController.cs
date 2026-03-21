using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Controllers.QuizMultiplayer;

[Route("QuizMultiplayer")]
public class QuizMultiplayerViewController : Controller
{
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;

    public QuizMultiplayerViewController(IMinigameFrontendRegistry minigameFrontendRegistry)
    {
        _minigameFrontendRegistry = minigameFrontendRegistry;
    }

    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Lobby()
    {
        ViewBag.MinigameTypeId = _minigameFrontendRegistry.GetDescriptor("QuizMultiplayer")?.MinigameTypeId ?? 3;
        return View("~/Views/QuizMultiplayer/Lobby.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Game(Guid gameId)
    {
        ViewBag.GameId = gameId;
        ViewBag.MinigameTypeId = _minigameFrontendRegistry.GetDescriptor("QuizMultiplayer")?.MinigameTypeId ?? 3;
        return View("~/Views/QuizMultiplayer/Game.cshtml");
    }
}
