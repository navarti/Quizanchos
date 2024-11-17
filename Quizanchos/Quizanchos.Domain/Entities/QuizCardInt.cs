using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardInt : QuizCardAbstract
{
    public Guid? Option1Id { get; set; }
    public FeatureInt Option1 { get; set; }
    public Guid? Option2Id { get; set; }
    public FeatureInt Option2 { get; set; }
}
