namespace Quizanchos.WebApi.Util;

public class UserException : Exception
{
    public UserException(string message) : base(message)
    {
    }

    public UserException(Exception exception)
    {
    }
}
