namespace Quizanchos.WebApi.Controllers;

public record GameStateResponse
{
    public Guid GameId { get; init; }
    public int MinigameType { get; init; }
    public IReadOnlyList<string> Players { get; init; } = Array.Empty<string>();
    public bool IsFinished { get; init; }
    public string? Winner { get; init; }
    public object State { get; init; } = null!;
}
