namespace Quizanchos.WebApi.Util;

public class QuizanchosException : Exception
{
    public QuizanchosException(string message) : base(message)
    {
    }

    public QuizanchosException(Exception exception)
    {
    }
}
