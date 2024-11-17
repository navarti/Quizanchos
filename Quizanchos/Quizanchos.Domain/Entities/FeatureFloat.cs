using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Entities;

public class FeatureFloat : FeatureAbstract
{
    public FeatureValueFloat Value { get; set; }
}
