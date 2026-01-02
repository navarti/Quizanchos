using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Util;

public static class GameLevelHelper
{
    public static double ToCoefficient(this GameLevel gameLevel)
    {
        return gameLevel switch
        {
            GameLevel.Easy => 1.0,
            GameLevel.Medium => 0.5,
            GameLevel.Hard => 0.2,
            _ => throw CriticalExceptionFactory.Create("Unexpected game level")
        };
    }
}
