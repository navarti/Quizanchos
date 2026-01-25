using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Controllers;

public record CreateGameResponse
{
    public Guid GameId { get; init; }
    public MinigameType MinigameType { get; init; }
    public object State { get; init; } = null!;
}
