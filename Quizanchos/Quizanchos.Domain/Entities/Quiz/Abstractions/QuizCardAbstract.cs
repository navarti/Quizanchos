using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.Quiz.Abstractions;

public abstract class QuizCardAbstract : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public int CardIndex { get; set; }
    public int CorrectOption { get; set; }
    public int? OptionPicked { get; set; }
    public DateTime CreationTime { get; set; }
}
