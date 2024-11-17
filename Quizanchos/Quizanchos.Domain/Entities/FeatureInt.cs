using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class FeatureInt : FeatureAbstract
{
    public FeatureValueInt Value { get; set; }
}
