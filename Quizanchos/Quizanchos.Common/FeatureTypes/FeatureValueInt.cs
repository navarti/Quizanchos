namespace Quizanchos.Common.FeatureTypes;

public class FeatureValueInt : FeatureValue
{
    public int Value { get; }

    public FeatureValueInt(int value)
    {
        Value = value;
    }
}
