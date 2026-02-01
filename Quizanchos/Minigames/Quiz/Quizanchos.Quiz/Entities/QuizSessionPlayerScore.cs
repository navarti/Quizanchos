using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities;

public class QuizSessionPlayerScore : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid QuizGameSessionStateId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int Score { get; set; }

    // Navigation properties
    public QuizGameSessionState QuizGameSessionState { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
