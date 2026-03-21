using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class GameSession : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public int MinigameType { get; set; }
    public bool IsActive { get; set; }
    public bool IsFinished { get; set; }
    public string? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Navigation properties
    public ApplicationUser? Winner { get; set; }
    public GameSessionState? State { get; set; }
    public ICollection<GameSessionPlayer> Players { get; set; } = new List<GameSessionPlayer>();
}
