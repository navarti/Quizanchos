using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;
using Quizanchos.ViewModels;

namespace Quizanchos.WebApi.ViewControllers;

[Route("Minigame")]
public class MinigameViewController : Controller
{
    private readonly IMinigameFrontendRegistry _frontendRegistry;

    public MinigameViewController(IMinigameFrontendRegistry frontendRegistry)
    {
        _frontendRegistry = frontendRegistry;
    }

    [HttpGet("{gameKey}")]
    public IActionResult Lobby(string gameKey)
    {
        var descriptor = _frontendRegistry.GetDescriptor(gameKey);
        if (descriptor == null)
            return NotFound();

        var model = BuildViewModel(descriptor, "lobby", null);
        return View("~/Views/Minigame/Lobby.cshtml", model);
    }

    [HttpGet("{gameKey}/{gameId:guid}")]
    public IActionResult Game(string gameKey, Guid gameId)
    {
        var descriptor = _frontendRegistry.GetDescriptor(gameKey);
        if (descriptor == null)
            return NotFound();

        var model = BuildViewModel(descriptor, "game", gameId);
        return View("~/Views/Minigame/Game.cshtml", model);
    }

    private static MinigameHostViewModel BuildViewModel(IMinigameFrontendDescriptor descriptor, string viewMode, Guid? gameId)
    {
        var isGame = viewMode.Equals("game", StringComparison.OrdinalIgnoreCase);

        return new MinigameHostViewModel
        {
            GameKey = descriptor.GameKey,
            DisplayName = descriptor.DisplayName,
            Description = descriptor.Description,
            LobbyUrl = descriptor.LobbyUrl,
            GameUrlTemplate = descriptor.GameUrlTemplate,
            MinigameTypeId = descriptor.MinigameTypeId,
            ViewMode = viewMode,
            GameId = gameId,
            Styles = isGame ? descriptor.GameStyles : descriptor.LobbyStyles,
            Scripts = isGame ? descriptor.GameScripts : descriptor.LobbyScripts
        };
    }
}
