using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class GameSessionState : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    public int MinigameType { get; set; }
    public string StateJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public GameSession GameSession { get; set; } = null!;
}
