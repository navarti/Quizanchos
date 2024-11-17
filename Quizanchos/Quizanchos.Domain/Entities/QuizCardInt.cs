using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardInt : QuizCardAbstract
{
    public FeatureInt? Option1 { get; set; }
    public FeatureInt? Option2 { get; set; }
}
