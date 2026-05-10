using Quizanchos.Core;

namespace Quizanchos.Plugin.CountryGuesser.GameLogic;

public sealed record CountryGuesserMove : GameMove
{
    /// <summary>Latitude of the player's click on the world map (degrees, [-90, 90]).</summary>
    public double? Lat { get; init; }

    /// <summary>Longitude of the player's click on the world map (degrees, [-180, 180]).</summary>
    public double? Lon { get; init; }
}
