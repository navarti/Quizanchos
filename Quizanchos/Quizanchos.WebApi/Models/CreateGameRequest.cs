using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Controllers;

public record CreateGameRequest
{
    public MinigameType MinigameType { get; init; }
    public List<Guid> PlayerIds { get; init; } = new();
    public Dictionary<string, object>? Parameters { get; init; }
}
