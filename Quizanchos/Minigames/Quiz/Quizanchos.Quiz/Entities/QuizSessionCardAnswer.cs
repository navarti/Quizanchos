using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities;

public class QuizSessionCardAnswer : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid QuizSessionCardId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int? OptionPicked { get; set; }
    public DateTime AnsweredAt { get; set; }

    // Navigation properties
    public QuizSessionCard QuizSessionCard { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
