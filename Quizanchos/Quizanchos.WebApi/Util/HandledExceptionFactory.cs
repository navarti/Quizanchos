namespace Quizanchos.WebApi.Util;

public static class HandledExceptionFactory
{
    public static QuizanchosException CreateIdNotFoundException<T>(T id, string entityName)
    {
        return new QuizanchosException($"No {entityName} with id {id?.ToString() ?? "null"} found");
    }

    public static QuizanchosException CreateNullException(string entityName)
    {
        return new QuizanchosException($"{entityName} is null");
    }

    public static QuizanchosException Create(string message)
    {
        return new QuizanchosException(message);
    }
}
