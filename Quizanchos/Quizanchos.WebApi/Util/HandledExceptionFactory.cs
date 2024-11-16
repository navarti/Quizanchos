namespace Quizanchos.WebApi.Util;

public static class HandledExceptionFactory
{
    public static QuizanchosException CreateIdNotFoundException(Guid id, string entityName)
    {
        return new QuizanchosException($"No {entityName} with id {id} found");
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
