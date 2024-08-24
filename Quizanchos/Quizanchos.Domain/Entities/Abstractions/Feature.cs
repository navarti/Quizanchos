using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.Abstractions;

public abstract class Feature : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid QuizCategoryId { get; set; }
    public QuizCategory QuizCategory { get; set; }

    public Guid QuizEntityId { get; set; }
    public QuizEntity QuizEntity { get; set; }
}
