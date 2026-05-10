namespace Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

public enum CaravanMpDirection
{
    Unset = 0,
    Ascending = 1,
    Descending = 2,
}

public sealed class CaravanMpColumnSlot
{
    public CaravanMpCard Card { get; set; } = new();
    public List<CaravanMpCard> Attached { get; set; } = new();

    public int EffectiveValue
    {
        get
        {
            if (!Card.IsNumber)
            {
                return 0;
            }
            int value = Card.NumericValue;
            int kingMultiplier = 1;
            foreach (var attached in Attached)
            {
                if (attached.Rank == CaravanMpRank.King)
                {
                    value += value * kingMultiplier;
                    kingMultiplier *= 2;
                }
            }
            return value;
        }
    }

    public CaravanMpSuit EffectiveSuit
    {
        get
        {
            var suit = Card.Suit;
            for (int i = Attached.Count - 1; i >= 0; i--)
            {
                if (Attached[i].Rank == CaravanMpRank.Queen)
                {
                    suit = Attached[i].Suit;
                    break;
                }
            }
            return suit;
        }
    }
}

public sealed class CaravanMpColumn
{
    public int Index { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public List<CaravanMpColumnSlot> Slots { get; set; } = new();
    public CaravanMpDirection Direction { get; set; } = CaravanMpDirection.Unset;

    public int Value => Slots.Sum(s => s.EffectiveValue);

    public bool IsSold => Value >= CaravanMpConstants.CaravanSellMin && Value <= CaravanMpConstants.CaravanSellMax;

    public bool IsBusted => Value > CaravanMpConstants.CaravanSellMax;

    public CaravanMpColumnSlot? Top => Slots.Count == 0 ? null : Slots[^1];
}
