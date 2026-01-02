namespace Quizanchos.Quiz.FeatureTypes;

public class FeatureValueFloat : FeatureValue
{
    public float Value { get; }

    public FeatureValueFloat(float value)
    {
        Value = value;
    }
}
