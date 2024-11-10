namespace Quizanchos.WebApi.Util;

public static class ExceptionFactory
{
    public static UserException CreateIdNotFoundException(Guid id, string entityName)
    {
        return new UserException($"No {entityName} with id {id} found");
    }

    public static UserException CreateNullException(string entityName)
    {
        return new UserException($"{entityName} is null");
    }

    public static UserException Create(string message)
    {
        return new UserException(message);
    }
}
