using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Controllers;

public record GameMoveResponse
{
    public bool Success { get; init; }
    public MinigameType MinigameType { get; init; }
    public object State { get; init; } = null!;
    public bool IsFinished { get; init; }
    public string? Winner { get; init; }
}
