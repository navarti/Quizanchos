using Quizanchos.Common.Util;
using Quizanchos.Quiz.Util;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class SkipTakeValidatorTests
{
    [Fact]
    public void Validate_PositiveSkipAndTake_DoesNotThrow()
    {
        SkipTakeValidator.Validate(5, 10);
    }

    [Fact]
    public void Validate_ZeroSkip_IsAllowed()
    {
        SkipTakeValidator.Validate(0, 1);
    }

    [Fact]
    public void Validate_NegativeSkip_Throws()
    {
        Assert.Throws<QuizanchosException>(() => SkipTakeValidator.Validate(-1, 10));
    }

    [Fact]
    public void Validate_ZeroTake_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SkipTakeValidator.Validate(0, 0));
    }

    [Fact]
    public void Validate_NegativeTake_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SkipTakeValidator.Validate(0, -10));
    }
}
