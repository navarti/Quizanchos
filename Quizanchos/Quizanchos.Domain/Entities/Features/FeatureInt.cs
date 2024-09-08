using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities.Features;

public class FeatureInt : Feature
{
    public FeatureValueInt Value { get; set; }
}
