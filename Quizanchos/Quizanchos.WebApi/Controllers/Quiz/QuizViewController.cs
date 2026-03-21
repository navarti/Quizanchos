using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Controllers.Quiz;

[Route("Quiz")]
public class QuizViewController : Controller
{
    private readonly IMinigameFrontendRegistry _minigameFrontendRegistry;
    private const string GameKey = "Quiz";

    public QuizViewController(IMinigameFrontendRegistry minigameFrontendRegistry)
    {
        _minigameFrontendRegistry = minigameFrontendRegistry;
    }

    [HttpGet("Setup/{categoryId:guid}")]
    [Authorize(Roles = "User")]
    public IActionResult Setup(Guid categoryId)
    {
        var descriptor = GetRequiredDescriptor();

        // Pass only the category ID to the view
        // The view will load all data via API
        ViewBag.CategoryId = categoryId;
        ViewBag.MinigameTypeId = descriptor.MinigameTypeId;
        ViewBag.GameUrlTemplate = descriptor.GameUrlTemplate;
        ViewBag.LobbyUrl = descriptor.LobbyUrl;
        return View("~/Views/Quiz/SessionSetup.cshtml");
    }

    [HttpGet("{gameId:guid}")]
    public IActionResult Game(Guid gameId)
    {
        var descriptor = GetRequiredDescriptor();

        // Pass only the game ID to the view
        // The view will load all data via API
        ViewBag.GameId = gameId;
        ViewBag.MinigameTypeId = descriptor.MinigameTypeId;
        ViewBag.GameUrlTemplate = descriptor.GameUrlTemplate;
        ViewBag.LobbyUrl = descriptor.LobbyUrl;
        return View("~/Views/Quiz/Game.cshtml");
    }

    private IMinigameFrontendDescriptor GetRequiredDescriptor()
    {
        return _minigameFrontendRegistry.GetDescriptor(GameKey)
            ?? throw new InvalidOperationException($"Frontend descriptor '{GameKey}' is not registered.");
    }
}
