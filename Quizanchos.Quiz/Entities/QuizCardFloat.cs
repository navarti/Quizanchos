using Quizanchos.Quiz.Entities.Abstractions;

namespace Quizanchos.Quiz.Entities;

public class QuizCardFloat : QuizCardAbstract
{
    public List<FeatureFloat> Options { get; set; }
}
