using System.ComponentModel.DataAnnotations.Schema;

namespace Quizanchos.Domain.Entities;

public class UserMinigameScore : Interfaces.IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public int MinigameType { get; set; }

    public int Score { get; set; }
}
