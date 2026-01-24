using Quizanchos.Quiz.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities.Abstractions;

public abstract class FeatureAbstract : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public QuizCategory QuizCategory { get; set; }
    public QuizEntity QuizEntity { get; set; }
}
