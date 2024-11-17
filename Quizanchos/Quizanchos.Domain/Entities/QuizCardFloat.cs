using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardFloat : QuizCardAbstract
{
    public FeatureFloat? Option1 { get; set; }
    public FeatureFloat? Option2 { get; set; }
}
