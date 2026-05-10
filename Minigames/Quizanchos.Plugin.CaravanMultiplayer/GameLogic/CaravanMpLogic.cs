using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

public sealed class CaravanMpLogic : IGameLogic<CaravanMpState, CaravanMpMove>
{
    private readonly int _seed;
    private readonly Dictionary<string, string> _nicknames;

    public CaravanMpLogic(int seed = 0, Dictionary<string, string>? nicknames = null)
    {
        _seed = seed == 0 ? Random.Shared.Next() : seed;
        _nicknames = nicknames ?? new Dictionary<string, string>();
    }

    public CaravanMpState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        if (players.Length != 2)
        {
            throw new InvalidOperationException(
                $"Caravan Multiplayer requires exactly 2 players, got {players.Length}");
        }

        var rnd = new Random(_seed);
        var state = new CaravanMpState
        {
            GameId = gameId,
            Players = players.ToList(),
            CurrentTurnIndex = 0,
            PlayerNicknames = new Dictionary<string, string>(_nicknames),
        };

        for (int i = 0; i < 2; i++)
        {
            var deck = BuildDeck(rnd);
            var hand = new List<CaravanMpCard>();
            for (int k = 0; k < CaravanMpConstants.HandSize && deck.Count > 0; k++)
            {
                hand.Add(Pop(deck));
            }
            state.PlayerStates.Add(new CaravanMpPlayerState
            {
                PlayerId = players[i],
                Hand = hand,
                Deck = deck,
            });
        }

        for (int playerIdx = 0; playerIdx < 2; playerIdx++)
        {
            for (int c = 0; c < CaravanMpConstants.CaravansPerPlayer; c++)
            {
                state.Columns.Add(new CaravanMpColumn
                {
                    Index = playerIdx * CaravanMpConstants.CaravansPerPlayer + c,
                    OwnerId = state.PlayerStates[playerIdx].PlayerId,
                });
            }
        }

        return state;
    }

    public MoveResult ValidateMove(CaravanMpState state, CaravanMpMove move, string playerId)
    {
        int playerIdx = IndexOf(state, playerId);
        if (playerIdx < 0)
        {
            return MoveResult.Failure("Player not in game");
        }
        if (state.CurrentTurnIndex != playerIdx)
        {
            return MoveResult.Failure("Not your turn");
        }
        var ps = state.PlayerStates[playerIdx];

        if (move.Type == CaravanMpMoveType.DiscardCaravan)
        {
            if (move.TargetColumnIndex < 0 || move.TargetColumnIndex >= state.Columns.Count)
            {
                return MoveResult.Failure("Invalid caravan");
            }
            var col = state.Columns[move.TargetColumnIndex];
            if (col.OwnerId != playerId)
            {
                return MoveResult.Failure("Cannot discard opponent's caravan");
            }
            if (col.Slots.Count == 0)
            {
                return MoveResult.Failure("Caravan is already empty");
            }
            return MoveResult.Success;
        }

        if (move.HandIndex < 0 || move.HandIndex >= ps.Hand.Count)
        {
            return MoveResult.Failure("Invalid hand index");
        }
        var card = ps.Hand[move.HandIndex];

        switch (move.Type)
        {
            case CaravanMpMoveType.DiscardCard:
                return MoveResult.Success;

            case CaravanMpMoveType.PlayNumber:
                {
                    if (!card.IsNumber)
                    {
                        return MoveResult.Failure("Card is not a number card");
                    }
                    if (move.TargetColumnIndex < 0 || move.TargetColumnIndex >= state.Columns.Count)
                    {
                        return MoveResult.Failure("Invalid caravan");
                    }
                    var col = state.Columns[move.TargetColumnIndex];
                    if (col.OwnerId != playerId)
                    {
                        return MoveResult.Failure("Cannot place number cards in opponent's caravan");
                    }
                    return ValidateNumberPlacement(col, card);
                }

            case CaravanMpMoveType.AttachFace:
                {
                    if (!card.IsFace)
                    {
                        return MoveResult.Failure("Card is not a face card");
                    }
                    if (move.TargetColumnIndex < 0 || move.TargetColumnIndex >= state.Columns.Count)
                    {
                        return MoveResult.Failure("Invalid caravan");
                    }
                    var col = state.Columns[move.TargetColumnIndex];
                    if (move.TargetSlotIndex < 0 || move.TargetSlotIndex >= col.Slots.Count)
                    {
                        return MoveResult.Failure("Invalid slot");
                    }
                    var slot = col.Slots[move.TargetSlotIndex];
                    if (card.Rank == CaravanMpRank.Joker && !(slot.Card.Rank == CaravanMpRank.Ace || slot.Card.IsNumber))
                    {
                        return MoveResult.Failure("Joker can only attach to a number card or ace");
                    }
                    return MoveResult.Success;
                }

            default:
                return MoveResult.Failure("Unknown move type");
        }
    }

    public void ApplyMove(CaravanMpState state, CaravanMpMove move, string playerId)
    {
        int playerIdx = IndexOf(state, playerId);
        var ps = state.PlayerStates[playerIdx];
        string desc;

        if (move.Type == CaravanMpMoveType.DiscardCaravan)
        {
            var col = state.Columns[move.TargetColumnIndex];
            foreach (var slot in col.Slots)
            {
                state.Discard.Add(slot.Card);
                state.Discard.AddRange(slot.Attached);
            }
            desc = $"{ShortName(state, playerId)} discarded caravan {ColumnLabel(col.Index)}";
            col.Slots.Clear();
            col.Direction = CaravanMpDirection.Unset;
        }
        else if (move.Type == CaravanMpMoveType.DiscardCard)
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            state.Discard.Add(card);
            desc = $"{ShortName(state, playerId)} discarded {card}";
        }
        else if (move.Type == CaravanMpMoveType.PlayNumber)
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            ApplyNumberPlacement(col, card);
            desc = $"{ShortName(state, playerId)} placed {card} on caravan {ColumnLabel(col.Index)}";
        }
        else
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            var slot = col.Slots[move.TargetSlotIndex];
            ApplyFaceAttachment(state, col, slot, card);
            desc = $"{ShortName(state, playerId)} attached {card} to {slot.Card}";
        }

        state.LastMoveDescription = desc;

        while (ps.Hand.Count < CaravanMpConstants.HandSize && ps.Deck.Count > 0)
        {
            ps.Hand.Add(Pop(ps.Deck));
        }

        state.CurrentTurnIndex = 1 - state.CurrentTurnIndex;
    }

    public bool CheckFinished(CaravanMpState state)
    {
        if (state.PlayerStates.Count < 2)
        {
            return true;
        }

        int p0Wins = 0;
        int p1Wins = 0;
        int decided = 0;
        for (int i = 0; i < CaravanMpConstants.CaravansPerPlayer; i++)
        {
            var p0Col = state.Columns[i];
            var p1Col = state.Columns[i + CaravanMpConstants.CaravansPerPlayer];

            bool p0Sold = p0Col.IsSold;
            bool p1Sold = p1Col.IsSold;

            if (!p0Sold && !p1Sold)
            {
                continue;
            }

            decided++;
            int p0Score = p0Sold ? p0Col.Value : 0;
            int p1Score = p1Sold ? p1Col.Value : 0;

            if (p0Score > p1Score) p0Wins++;
            else if (p1Score > p0Score) p1Wins++;
        }

        if (p0Wins >= 2 || p1Wins >= 2)
        {
            return true;
        }

        bool anyCardsLeft = state.PlayerStates.Any(p => p.Hand.Count > 0 || p.Deck.Count > 0);
        return !anyCardsLeft && decided == CaravanMpConstants.CaravansPerPlayer;
    }

    public string? DetermineWinner(CaravanMpState state)
    {
        if (state.PlayerStates.Count < 2)
        {
            return null;
        }

        int p0Wins = 0;
        int p1Wins = 0;
        for (int i = 0; i < CaravanMpConstants.CaravansPerPlayer; i++)
        {
            var p0Col = state.Columns[i];
            var p1Col = state.Columns[i + CaravanMpConstants.CaravansPerPlayer];
            int p0 = p0Col.IsSold ? p0Col.Value : 0;
            int p1 = p1Col.IsSold ? p1Col.Value : 0;
            if (p0 > p1) p0Wins++;
            else if (p1 > p0) p1Wins++;
        }

        if (p0Wins > p1Wins) return state.PlayerStates[0].PlayerId;
        if (p1Wins > p0Wins) return state.PlayerStates[1].PlayerId;
        return null;
    }

    public IEnumerable<string> GetExpectedPlayers(CaravanMpState state)
    {
        if (state.PlayerStates.Count == 0 || state.IsFinished)
        {
            yield break;
        }
        var current = state.PlayerStates[state.CurrentTurnIndex];
        yield return current.PlayerId;
    }

    public bool NeedToFinish(CaravanMpState state) => false;

    public IReadOnlyDictionary<string, int> GetPlayerScores(CaravanMpState state)
    {
        var scores = new Dictionary<string, int>();
        if (!state.IsFinished || state.PlayerStates.Count < 2)
        {
            return scores;
        }

        var winnerId = state.Winner;
        bool isDraw = string.IsNullOrEmpty(winnerId);
        foreach (var ps in state.PlayerStates)
        {
            int points = isDraw ? 1 : (ps.PlayerId == winnerId ? 3 : 0);
            scores[ps.PlayerId] = points;
        }
        return scores;
    }

    private static MoveResult ValidateNumberPlacement(CaravanMpColumn col, CaravanMpCard card)
    {
        if (col.Slots.Count == 0)
        {
            return MoveResult.Success;
        }

        var top = col.Top!;
        int topValue = top.Card.NumericValue;
        int newValue = card.NumericValue;

        if (newValue == topValue)
        {
            return MoveResult.Failure("Cannot stack a card of equal value");
        }

        bool sameSuit = card.Suit == top.EffectiveSuit;
        if (col.Slots.Count == 1)
        {
            return MoveResult.Success;
        }

        bool ascending = newValue > topValue;
        if (col.Direction == CaravanMpDirection.Ascending && !ascending && !sameSuit)
        {
            return MoveResult.Failure("Caravan is ascending; play a higher card or match suit to reverse");
        }
        if (col.Direction == CaravanMpDirection.Descending && ascending && !sameSuit)
        {
            return MoveResult.Failure("Caravan is descending; play a lower card or match suit to reverse");
        }

        return MoveResult.Success;
    }

    private static void ApplyNumberPlacement(CaravanMpColumn col, CaravanMpCard card)
    {
        if (col.Slots.Count >= 1)
        {
            var top = col.Top!;
            int topValue = top.Card.NumericValue;
            int newValue = card.NumericValue;
            bool ascending = newValue > topValue;
            bool sameSuit = card.Suit == top.EffectiveSuit;

            if (col.Slots.Count == 1)
            {
                col.Direction = ascending ? CaravanMpDirection.Ascending : CaravanMpDirection.Descending;
            }
            else if (sameSuit)
            {
                if (col.Direction == CaravanMpDirection.Ascending && !ascending)
                {
                    col.Direction = CaravanMpDirection.Descending;
                }
                else if (col.Direction == CaravanMpDirection.Descending && ascending)
                {
                    col.Direction = CaravanMpDirection.Ascending;
                }
            }
        }

        col.Slots.Add(new CaravanMpColumnSlot { Card = card });
    }

    private static void ApplyFaceAttachment(CaravanMpState state, CaravanMpColumn col, CaravanMpColumnSlot slot, CaravanMpCard card)
    {
        switch (card.Rank)
        {
            case CaravanMpRank.Jack:
                {
                    state.Discard.Add(slot.Card);
                    state.Discard.AddRange(slot.Attached);
                    state.Discard.Add(card);
                    col.Slots.Remove(slot);
                    if (col.Slots.Count < 2)
                    {
                        col.Direction = CaravanMpDirection.Unset;
                    }
                    break;
                }
            case CaravanMpRank.Queen:
                {
                    slot.Attached.Add(card);
                    if (col.Slots.Count >= 2 && ReferenceEquals(col.Top, slot))
                    {
                        col.Direction = col.Direction switch
                        {
                            CaravanMpDirection.Ascending => CaravanMpDirection.Descending,
                            CaravanMpDirection.Descending => CaravanMpDirection.Ascending,
                            _ => CaravanMpDirection.Unset,
                        };
                    }
                    break;
                }
            case CaravanMpRank.King:
                {
                    slot.Attached.Add(card);
                    break;
                }
            case CaravanMpRank.Joker:
                {
                    slot.Attached.Add(card);
                    if (slot.Card.Rank == CaravanMpRank.Ace)
                    {
                        RemoveBySuit(state, slot, slot.Card.Suit);
                    }
                    else
                    {
                        RemoveByValue(state, slot, slot.Card.NumericValue);
                    }
                    break;
                }
        }
    }

    private static void RemoveBySuit(CaravanMpState state, CaravanMpColumnSlot host, CaravanMpSuit suit)
    {
        foreach (var col in state.Columns)
        {
            for (int i = col.Slots.Count - 1; i >= 0; i--)
            {
                var slot = col.Slots[i];
                if (ReferenceEquals(slot, host)) continue;
                if (slot.Card.Suit == suit)
                {
                    state.Discard.Add(slot.Card);
                    state.Discard.AddRange(slot.Attached);
                    col.Slots.RemoveAt(i);
                }
            }
            if (col.Slots.Count < 2)
            {
                col.Direction = CaravanMpDirection.Unset;
            }
        }
    }

    private static void RemoveByValue(CaravanMpState state, CaravanMpColumnSlot host, int value)
    {
        foreach (var col in state.Columns)
        {
            for (int i = col.Slots.Count - 1; i >= 0; i--)
            {
                var slot = col.Slots[i];
                if (ReferenceEquals(slot, host)) continue;
                if (slot.Card.IsNumber && slot.Card.NumericValue == value)
                {
                    state.Discard.Add(slot.Card);
                    state.Discard.AddRange(slot.Attached);
                    col.Slots.RemoveAt(i);
                }
            }
            if (col.Slots.Count < 2)
            {
                col.Direction = CaravanMpDirection.Unset;
            }
        }
    }

    private static int IndexOf(CaravanMpState state, string playerId)
    {
        for (int i = 0; i < state.PlayerStates.Count; i++)
        {
            if (state.PlayerStates[i].PlayerId == playerId)
                return i;
        }
        return -1;
    }

    private static List<CaravanMpCard> BuildDeck(Random rnd)
    {
        var cards = new List<CaravanMpCard>();
        for (int s = 0; s < 4; s++)
        {
            for (int r = (int)CaravanMpRank.Ace; r <= (int)CaravanMpRank.King; r++)
            {
                cards.Add(new CaravanMpCard { Suit = (CaravanMpSuit)s, Rank = (CaravanMpRank)r });
            }
        }
        cards.Add(new CaravanMpCard { Suit = CaravanMpSuit.Hearts, Rank = CaravanMpRank.Joker });
        cards.Add(new CaravanMpCard { Suit = CaravanMpSuit.Spades, Rank = CaravanMpRank.Joker });

        Shuffle(cards, rnd);
        return cards.Take(CaravanMpConstants.DeckSize).ToList();
    }

    private static void Shuffle(List<CaravanMpCard> list, Random rnd)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static CaravanMpCard Pop(List<CaravanMpCard> deck)
    {
        var card = deck[^1];
        deck.RemoveAt(deck.Count - 1);
        return card;
    }

    private static string ColumnLabel(int index)
    {
        return ((char)('A' + index)).ToString();
    }

    private static string ShortName(CaravanMpState state, string playerId)
    {
        if (state.PlayerNicknames.TryGetValue(playerId, out var nick) && !string.IsNullOrEmpty(nick))
        {
            return nick;
        }
        return playerId.Length <= 8 ? playerId : playerId.Substring(0, 8);
    }
}
