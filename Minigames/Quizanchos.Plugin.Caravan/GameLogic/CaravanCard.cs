namespace Quizanchos.Plugin.Caravan.GameLogic;

public enum CaravanSuit
{
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3,
}

public enum CaravanRank
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

public sealed class CaravanCard
{
    public CaravanSuit Suit { get; set; }
    public CaravanRank Rank { get; set; }

    public bool IsFace => Rank is CaravanRank.Jack or CaravanRank.Queen or CaravanRank.King or CaravanRank.Joker;
    public bool IsNumber => Rank is >= CaravanRank.Ace and <= CaravanRank.Ten;

    public int NumericValue => IsNumber ? (int)Rank : 0;

    public string Code => $"{(int)Rank:D2}{(int)Suit}";

    public override string ToString()
    {
        var rankStr = Rank switch
        {
            CaravanRank.Ace => "A",
            CaravanRank.Jack => "J",
            CaravanRank.Queen => "Q",
            CaravanRank.King => "K",
            CaravanRank.Joker => "JK",
            _ => ((int)Rank).ToString(),
        };
        var suitStr = Suit switch
        {
            CaravanSuit.Hearts => "H",
            CaravanSuit.Diamonds => "D",
            CaravanSuit.Clubs => "C",
            CaravanSuit.Spades => "S",
            _ => "?",
        };
        return rankStr + suitStr;
    }
}
