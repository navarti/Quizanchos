using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardInt : QuizCardAbstract
{
    public List<FeatureInt> Options { get; set; }
}
