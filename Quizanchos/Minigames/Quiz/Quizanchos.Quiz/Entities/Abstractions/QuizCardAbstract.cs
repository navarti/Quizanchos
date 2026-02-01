using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities.Abstractions;

public abstract class QuizCardAbstract : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public int CardIndex { get; set; }
    // TODO: make many to many
    public int CorrectOption { get; set; }
    public int? OptionPicked { get; set; }
    public DateTime CreationTime { get; set; }
}
