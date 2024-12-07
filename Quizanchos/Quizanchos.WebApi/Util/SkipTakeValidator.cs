using Quizanchos.Common.Util;

namespace Quizanchos.WebApi.Util;

public static class SkipTakeValidator
{
    public static void Validate(int skip, int take)
    {
        if (skip < 0)
        {
            throw HandledExceptionFactory.Create("Skip must be >= 0");
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be > 0");
        }
    }
}
