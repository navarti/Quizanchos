using Quizanchos.Core;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class MoveResultTests
{
    [Fact]
    public void Success_IsSuccessful()
    {
        Assert.True(MoveResult.Success.IsSuccess);
        Assert.Equal(string.Empty, MoveResult.Success.Reason);
    }

    [Fact]
    public void Failure_HasMatchingReason()
    {
        var result = MoveResult.Failure("boom");

        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.Reason);
    }

    [Fact]
    public void Failure_ProducesDistinctInstancesByReason()
    {
        var a = MoveResult.Failure("a");
        var b = MoveResult.Failure("b");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Records_AreEqual_WhenContentMatches()
    {
        var a = MoveResult.Failure("same");
        var b = MoveResult.Failure("same");

        Assert.Equal(a, b);
    }
}
