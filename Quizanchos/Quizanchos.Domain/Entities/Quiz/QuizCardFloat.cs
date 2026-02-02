using Quizanchos.Domain.Entities.Quiz.Abstractions;

namespace Quizanchos.Domain.Entities.Quiz;

public class QuizCardFloat : QuizCardAbstract
{
    public List<FeatureFloat> Options { get; set; }
}
