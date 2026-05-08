namespace Quizanchos.Plugin.Caravan.GameLogic;

public enum CaravanDirection
{
    Unset = 0,
    Ascending = 1,
    Descending = 2,
}

public sealed class CaravanColumnSlot
{
    public CaravanCard Card { get; set; } = new();
    public List<CaravanCard> Attached { get; set; } = new();

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
                if (attached.Rank == CaravanRank.King)
                {
                    value += value * kingMultiplier;
                    kingMultiplier *= 2;
                }
            }
            return value;
        }
    }

    public CaravanSuit EffectiveSuit
    {
        get
        {
            var suit = Card.Suit;
            for (int i = Attached.Count - 1; i >= 0; i--)
            {
                if (Attached[i].Rank == CaravanRank.Queen)
                {
                    suit = Attached[i].Suit;
                    break;
                }
            }
            return suit;
        }
    }
}

public sealed class CaravanColumn
{
    public int Index { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public List<CaravanColumnSlot> Slots { get; set; } = new();
    public CaravanDirection Direction { get; set; } = CaravanDirection.Unset;

    public int Value => Slots.Sum(s => s.EffectiveValue);

    public bool IsSold => Value >= CaravanConstants.CaravanSellMin && Value <= CaravanConstants.CaravanSellMax;

    public bool IsBusted => Value > CaravanConstants.CaravanSellMax;

    public CaravanColumnSlot? Top => Slots.Count == 0 ? null : Slots[^1];
}
