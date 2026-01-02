using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

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
