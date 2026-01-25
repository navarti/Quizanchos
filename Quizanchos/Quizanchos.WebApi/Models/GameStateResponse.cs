using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Controllers;

public record GameStateResponse
{
    public Guid GameId { get; init; }
    public MinigameType MinigameType { get; init; }
    public IReadOnlyList<Guid> Players { get; init; } = Array.Empty<Guid>();
    public bool IsFinished { get; init; }
    public Guid? Winner { get; init; }
    public object State { get; init; } = null!;
}
