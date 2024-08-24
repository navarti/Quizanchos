using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class QuizEntity : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}
