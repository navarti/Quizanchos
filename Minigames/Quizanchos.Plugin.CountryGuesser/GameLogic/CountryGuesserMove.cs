using Quizanchos.Core;

namespace Quizanchos.Plugin.CountryGuesser.GameLogic;

public sealed record CountryGuesserMove : GameMove
{
    /// <summary>
    /// Index (0-based) of the option chosen for the current card.
    /// </summary>
    public int OptionPicked { get; init; } = -1;
}
