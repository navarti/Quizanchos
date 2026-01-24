namespace Quizanchos.Quiz.Util;

public class CriticalExceptionFactory
{
    public static Exception Create(string errorMessage)
    {
        throw new Exception(errorMessage);
    }

    public static ArgumentException CreateConfigException(string configPropertyName)
    {
        throw new ArgumentException($"Configuration property '{configPropertyName}' is missing");
    }
}
