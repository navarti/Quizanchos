using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.QuizMultiplayer;

public class QuizMultiplayerSessionState : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }

    /// <summary>
    /// Full game state serialized as JSON (cards, team scores, team assignments, etc.).
    /// </summary>
    public string StateJson { get; set; } = "{}";
    public DateTime CreationTime { get; set; }

    // Navigation properties
    public GameSession GameSession { get; set; } = null!;
}
