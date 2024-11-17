using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardFloat : QuizCardAbstract
{
    public Guid? Option1Id { get; set; }
    public FeatureFloat? Option1 { get; set; }
    public Guid? Option2Id { get; set; }
    public FeatureFloat? Option2 { get; set; }
}
