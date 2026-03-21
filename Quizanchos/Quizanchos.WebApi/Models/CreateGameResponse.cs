namespace Quizanchos.WebApi.Controllers;

public record CreateGameResponse
{
    public Guid GameId { get; init; }
    public int MinigameType { get; init; }
    public object State { get; init; } = null!;
}
