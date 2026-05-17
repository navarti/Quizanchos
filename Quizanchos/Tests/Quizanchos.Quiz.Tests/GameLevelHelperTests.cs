using Quizanchos.Common.Enums;
using Quizanchos.Quiz.Util;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class GameLevelHelperTests
{
    [Fact]
    public void ToCoefficient_Easy_Returns1()
    {
        Assert.Equal(1.0, GameLevel.Easy.ToCoefficient());
    }

    [Fact]
    public void ToCoefficient_Medium_ReturnsHalf()
    {
        Assert.Equal(0.5, GameLevel.Medium.ToCoefficient());
    }

    [Fact]
    public void ToCoefficient_Hard_ReturnsZeroPointTwo()
    {
        Assert.Equal(0.2, GameLevel.Hard.ToCoefficient());
    }

    [Fact]
    public void ToCoefficient_InvalidEnumValue_Throws()
    {
        Assert.Throws<Exception>(() => ((GameLevel)999).ToCoefficient());
    }
}
