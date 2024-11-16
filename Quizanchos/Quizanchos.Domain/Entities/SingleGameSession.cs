using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class SingleGameSession : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }

    public ApplicationUser ApplicationUser { get; set; }
    public QuizCategory QuizCategory { get; set; }
}
