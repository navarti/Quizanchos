using Quizanchos.Quiz.Util;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class CorrectOptionPickerTests
{
    [Fact]
    public void PickCorrectOption_ReturnsIndexOfMax_ForIntArray()
    {
        var items = new[] { 10, 50, 20, 30 };

        var index = CorrectOptionPicker.PickCorrectOption(items);

        Assert.Equal(1, index);
    }

    [Fact]
    public void PickCorrectOption_ReturnsIndexOfMax_ForFloatArray()
    {
        var items = new[] { 1.5f, 2.7f, 9.9f, 0.1f };

        var index = CorrectOptionPicker.PickCorrectOption(items);

        Assert.Equal(2, index);
    }

    [Fact]
    public void PickCorrectOption_SingleItem_ReturnsZero()
    {
        var items = new[] { 42 };

        var index = CorrectOptionPicker.PickCorrectOption(items);

        Assert.Equal(0, index);
    }

    [Fact]
    public void PickCorrectOption_OnEmptyCollection_Throws()
    {
        Assert.Throws<ArgumentException>(() => CorrectOptionPicker.PickCorrectOption(Array.Empty<int>()));
    }

    [Fact]
    public void PickCorrectOption_OnTie_ReturnsFirstMaxIndex()
    {
        var items = new[] { 5, 9, 9, 1 };

        var index = CorrectOptionPicker.PickCorrectOption(items);

        Assert.Equal(1, index);
    }

    [Fact]
    public void PickCorrectOption_HandlesNegativeValues()
    {
        var items = new[] { -10, -3, -7, -1 };

        var index = CorrectOptionPicker.PickCorrectOption(items);

        Assert.Equal(3, index);
    }
}
