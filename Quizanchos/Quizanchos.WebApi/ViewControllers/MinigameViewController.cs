using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;
using Quizanchos.ViewModels;
using Quizanchos.WebApi.Services.Users;
using System.Security.Claims;

namespace Quizanchos.WebApi.ViewControllers;

[Route("Minigame")]
public class MinigameViewController : Controller
{
    private readonly IMinigameFrontendRegistry _frontendRegistry;
    private readonly PremiumAccessService _premiumAccessService;

    public MinigameViewController(IMinigameFrontendRegistry frontendRegistry, PremiumAccessService premiumAccessService)
    {
        _frontendRegistry = frontendRegistry;
        _premiumAccessService = premiumAccessService;
    }

    [HttpGet("{gameKey}")]
    public async Task<IActionResult> Lobby(string gameKey)
    {
        var descriptor = _frontendRegistry.GetDescriptor(gameKey);
        if (descriptor == null)
            return NotFound();

        var accessResult = await EnsurePremiumAccessIfRequired(descriptor);
        if (accessResult is not null)
            return accessResult;

        var model = BuildViewModel(descriptor, "lobby", null);
        return View("~/Views/Minigame/Lobby.cshtml", model);
    }

    [HttpGet("{gameKey}/{gameId:guid}")]
    public async Task<IActionResult> Game(string gameKey, Guid gameId)
    {
        var descriptor = _frontendRegistry.GetDescriptor(gameKey);
        if (descriptor == null)
            return NotFound();

        var accessResult = await EnsurePremiumAccessIfRequired(descriptor);
        if (accessResult is not null)
            return accessResult;

        var model = BuildViewModel(descriptor, "game", gameId);
        return View("~/Views/Minigame/Game.cshtml", model);
    }

    private async Task<IActionResult?> EnsurePremiumAccessIfRequired(IMinigameFrontendDescriptor descriptor)
    {
        if (!descriptor.IsPremium)
        {
            return null;
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await _premiumAccessService
            .EnsureUsersCanAccessMinigameAsync([userId], descriptor.MinigameTypeId)
            .ConfigureAwait(false);

        return null;
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
