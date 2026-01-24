namespace Quizanchos.Common.FeatureTypes;

public class FeatureValueFloat : FeatureValue
{
    public float Value { get; }

    public FeatureValueFloat(float value)
    {
        Value = value;
    }
}
