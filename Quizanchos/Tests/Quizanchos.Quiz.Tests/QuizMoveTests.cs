using System.Text.Json;
using Quizanchos.Quiz.GameLogic;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class QuizMoveTests
{
    [Fact]
    public void DefaultConstructor_SetsOptionToZero()
    {
        var move = new QuizMove();

        Assert.Equal(0, move.OptionPicked);
    }

    [Fact]
    public void ConstructorWithOption_SetsOptionAccordingly()
    {
        var move = new QuizMove(3);

        Assert.Equal(3, move.OptionPicked);
    }

    [Fact]
    public void Equality_ByValue()
    {
        var a = new QuizMove(2);
        var b = new QuizMove(2);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Inequality_ByOption()
    {
        var a = new QuizMove(1);
        var b = new QuizMove(2);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RoundtripSerialization_PreservesOptionPicked()
    {
        var move = new QuizMove(5);

        var json = JsonSerializer.Serialize(move);
        var restored = JsonSerializer.Deserialize<QuizMove>(json);

        Assert.NotNull(restored);
        Assert.Equal(5, restored!.OptionPicked);
    }
}
