using Quizanchos.Quiz.Entities.Abstractions;

namespace Quizanchos.Quiz.Entities;

public class QuizCardInt : QuizCardAbstract
{
    public List<FeatureInt> Options { get; set; }
}
