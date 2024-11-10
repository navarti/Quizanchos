namespace Quizanchos.WebApi.Util;

public static class ExceptionFactory
{
    public static ArgumentException CreateIdNotFoundException(Guid id, string entityName)
    {
        return new ArgumentException(
            paramName: nameof(id),
            message: $"No {entityName} with id {id} found");
    }

    public static ArgumentNullException CreateNullException(string entityName)
    {
        return new ArgumentNullException($"{entityName} is null");
    }

    public static Exception Create(string message)
    {
        return new Exception(message);
    }
}
