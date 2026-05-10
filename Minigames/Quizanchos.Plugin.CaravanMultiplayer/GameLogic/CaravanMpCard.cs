namespace Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

public enum CaravanMpSuit
{
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3,
}

public enum CaravanMpRank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Joker = 14,
}

public sealed class CaravanMpCard
{
    public CaravanMpSuit Suit { get; set; }
    public CaravanMpRank Rank { get; set; }

    public bool IsFace => Rank is CaravanMpRank.Jack or CaravanMpRank.Queen or CaravanMpRank.King or CaravanMpRank.Joker;
    public bool IsNumber => Rank is >= CaravanMpRank.Ace and <= CaravanMpRank.Ten;

    public int NumericValue => IsNumber ? (int)Rank : 0;

    public override string ToString()
    {
        var rankStr = Rank switch
        {
            CaravanMpRank.Ace => "A",
            CaravanMpRank.Jack => "J",
            CaravanMpRank.Queen => "Q",
            CaravanMpRank.King => "K",
            CaravanMpRank.Joker => "JK",
            _ => ((int)Rank).ToString(),
        };
        var suitStr = Suit switch
        {
            CaravanMpSuit.Hearts => "H",
            CaravanMpSuit.Diamonds => "D",
            CaravanMpSuit.Clubs => "C",
            CaravanMpSuit.Spades => "S",
            _ => "?",
        };
        return rankStr + suitStr;
    }
}
