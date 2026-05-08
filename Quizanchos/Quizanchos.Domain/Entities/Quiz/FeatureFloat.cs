using Quizanchos.Domain.Quiz.FeatureTypes;
using Quizanchos.Domain.Entities.Quiz.Abstractions;

namespace Quizanchos.Domain.Entities.Quiz;

public class FeatureFloat : FeatureAbstract, IComparable<FeatureFloat>
{
    public FeatureValueFloat Value { get; set; }

    public int CompareTo(FeatureFloat? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Value.Value.CompareTo(other.Value.Value);
    }
}
