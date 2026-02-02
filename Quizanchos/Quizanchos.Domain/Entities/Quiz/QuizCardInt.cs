using Quizanchos.Domain.Entities.Quiz.Abstractions;

namespace Quizanchos.Domain.Entities.Quiz;

public class QuizCardInt : QuizCardAbstract
{
    public List<FeatureInt> Options { get; set; }
}
