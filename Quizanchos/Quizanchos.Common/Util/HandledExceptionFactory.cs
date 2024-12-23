﻿namespace Quizanchos.Common.Util;

public static class HandledExceptionFactory
{
    public static QuizanchosException CreateIdNotFoundException<T>(T id)
    {
        return new QuizanchosException($"No entity with id {id?.ToString() ?? "null"} found");
    }

    public static QuizanchosException CreateNullException(string entityName)
    {
        return new QuizanchosException($"{entityName} is null");
    }

    public static QuizanchosException Create(string message)
    {
        return new QuizanchosException(message);
    }

    public static QuizanchosException CreateForbiddenException()
    {
        return new QuizanchosException("Forbidden");
    }
}
