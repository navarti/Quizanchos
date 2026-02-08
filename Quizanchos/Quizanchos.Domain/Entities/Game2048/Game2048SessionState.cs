using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.Game2048;

public class Game2048SessionState : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }

    public int Size { get; set; }
    public string BoardJson { get; set; } = "[]";
    public int Score { get; set; }
    public int BestTile { get; set; }
    public int MoveCount { get; set; }
    public DateTime CreationTime { get; set; }

    // Navigation properties
    public GameSession GameSession { get; set; } = null!;
}
