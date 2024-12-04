using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class QuizCardFloat : QuizCardAbstract
{
    public List<FeatureFloat> Options { get; set; }
}
