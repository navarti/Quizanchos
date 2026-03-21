using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Controllers.QuizMultiplayer;

[Route("QuizMultiplayer")]
public class QuizMultiplayerViewController : Controller
{
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;
    private const string GameKey = "QuizMultiplayer";

    public QuizMultiplayerViewController(IMinigameFrontendRegistry minigameFrontendRegistry)
    {
        _minigameFrontendRegistry = minigameFrontendRegistry;
    }

    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Lobby()
    {
        var descriptor = GetRequiredDescriptor();
        ViewBag.MinigameTypeId = descriptor.MinigameTypeId;
        ViewBag.GameUrlTemplate = descriptor.GameUrlTemplate;
        ViewBag.LobbyUrl = descriptor.LobbyUrl;
        return View("~/Views/QuizMultiplayer/Lobby.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Game(Guid gameId)
    {
        var descriptor = GetRequiredDescriptor();
        ViewBag.GameId = gameId;
        ViewBag.MinigameTypeId = descriptor.MinigameTypeId;
        ViewBag.GameUrlTemplate = descriptor.GameUrlTemplate;
        ViewBag.LobbyUrl = descriptor.LobbyUrl;
        return View("~/Views/QuizMultiplayer/Game.cshtml");
    }

    private IMinigameFrontendDescriptor GetRequiredDescriptor()
    {
        return _minigameFrontendRegistry.GetDescriptor(GameKey)
            ?? throw new InvalidOperationException($"Frontend descriptor '{GameKey}' is not registered.");
    }
}
