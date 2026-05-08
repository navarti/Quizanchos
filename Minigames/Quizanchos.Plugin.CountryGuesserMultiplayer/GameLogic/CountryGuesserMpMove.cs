using Quizanchos.Core;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;

public sealed record CountryGuesserMpMove : GameMove
{
    public int OptionPicked { get; init; } = -1;
}
