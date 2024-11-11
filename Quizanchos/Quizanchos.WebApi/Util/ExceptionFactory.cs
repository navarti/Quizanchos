namespace Quizanchos.WebApi.Util;

public static class ExceptionFactory
{
    public static ApiException CreateIdNotFoundException(Guid id, string entityName)
    {
        return new ApiException($"No {entityName} with id {id} found");
    }

    public static ApiException CreateNullException(string entityName)
    {
        return new ApiException($"{entityName} is null");
    }

    public static ApiException Create(string message)
    {
        return new ApiException(message);
    }
}
