using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.Abstractions;

public abstract class Feature<T> : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public T Value { get; set; }

    public QuizCategory QuizCategory { get; set; }
    public QuizEntity QuizEntity { get; set; }
}
