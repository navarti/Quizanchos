using Quizanchos.Common.Util;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class ExceptionTests
{
    [Fact]
    public void QuizanchosException_CarriesMessage()
    {
        var ex = new QuizanchosException("kaboom");

        Assert.Equal("kaboom", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void QuizanchosException_WrapsInnerException()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new QuizanchosException(inner);

        Assert.Equal("inner", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void HandledExceptionFactory_Create_ReturnsQuizanchosException()
    {
        var ex = HandledExceptionFactory.Create("nope");

        Assert.IsType<QuizanchosException>(ex);
        Assert.Equal("nope", ex.Message);
    }

    [Fact]
    public void HandledExceptionFactory_IdNotFound_IncludesId()
    {
        var id = Guid.NewGuid();

        var ex = HandledExceptionFactory.CreateIdNotFoundException(id);

        Assert.Contains(id.ToString(), ex.Message);
    }

    [Fact]
    public void HandledExceptionFactory_IdNotFound_HandlesNull()
    {
        var ex = HandledExceptionFactory.CreateIdNotFoundException<string?>(null);

        Assert.Contains("null", ex.Message);
    }

    [Fact]
    public void HandledExceptionFactory_NullException_IncludesEntityName()
    {
        var ex = HandledExceptionFactory.CreateNullException("User");

        Assert.Contains("User", ex.Message);
    }

    [Fact]
    public void HandledExceptionFactory_Forbidden_MessageIsForbidden()
    {
        var ex = HandledExceptionFactory.CreateForbiddenException();

        Assert.Equal("Forbidden", ex.Message);
    }
}
