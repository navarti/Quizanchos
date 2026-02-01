using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class GameSessionPlayer : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public GameSession GameSession { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
