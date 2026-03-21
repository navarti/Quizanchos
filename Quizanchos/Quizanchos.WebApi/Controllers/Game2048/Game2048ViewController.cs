using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Controllers.Game2048;

[Route("Game2048")]
public class Game2048ViewController : Controller
{
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;
    private const string GameKey = "Game2048";

    public Game2048ViewController(IMinigameFrontendRegistry minigameFrontendRegistry)
    {
        _minigameFrontendRegistry = minigameFrontendRegistry;
    }

    [HttpGet("")]
    [Authorize(Roles = "User")]
    public IActionResult Index()
    {
        var descriptor = GetRequiredDescriptor();
        ViewBag.MinigameTypeId = descriptor.MinigameTypeId;
        ViewBag.GameUrlTemplate = descriptor.GameUrlTemplate;
        ViewBag.LobbyUrl = descriptor.LobbyUrl;
        return View("~/Views/Game2048/Index.cshtml");
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
        return View("~/Views/Game2048/Game.cshtml");
    }

    private IMinigameFrontendDescriptor GetRequiredDescriptor()
    {
        return _minigameFrontendRegistry.GetDescriptor(GameKey)
            ?? throw new InvalidOperationException($"Frontend descriptor '{GameKey}' is not registered.");
    }
}
