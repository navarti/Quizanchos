using Quizanchos.Quiz.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities;

public class QuizEntity : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}
