using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class FeatureInt : FeatureAbstract, IComparable<FeatureInt>
{
    public FeatureValueInt Value { get; set; }

    public int CompareTo(FeatureInt? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Value.Value.CompareTo(other.Value.Value);
    }
}
