namespace Quizanchos.WebApi.Controllers;

public record GameMoveResponse
{
    public bool Success { get; init; }
    public int MinigameType { get; init; }
    public object State { get; init; } = null!;
    public bool IsFinished { get; init; }
    public string? Winner { get; init; }
}
