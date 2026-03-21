namespace Quizanchos.WebApi.Controllers;

public record CreateGameRequest
{
    public int MinigameType { get; init; }
    public List<string> PlayerIds { get; init; } = new();
    public Dictionary<string, object>? Parameters { get; init; }
}
