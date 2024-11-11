namespace Quizanchos.WebApi.Util;

public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }

    public ApiException(Exception exception)
    {
    }
}
