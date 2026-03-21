namespace Quizanchos.WebApi.Controllers;

public record CreateGameRequest
{
    public int MinigameType { get; init; }
    public List<string> PlayerIds { get; init; } = new();

    /// <summary>
    /// Minigame-specific parameters bag. Values can be sent as numbers/strings/JSON,
    /// and are parsed by each minigame descriptor.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; init; }
}
