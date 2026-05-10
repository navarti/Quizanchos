using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.Plugin.Caravan.GameLogic;

public sealed class CaravanLogic : IGameLogic<CaravanState, CaravanMove>
{
    private readonly int _seed;

    public CaravanLogic(int seed = 0)
    {
        _seed = seed == 0 ? Random.Shared.Next() : seed;
    }

    public CaravanState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        // Caravan is 1v1. If only one human player is supplied, we add an AI.
        var humans = players.ToList();
        var allPlayers = humans.Count == 1
            ? new List<string> { humans[0], CaravanConstants.AiPlayerId }
            : humans.Take(2).ToList();

        var rnd = new Random(_seed);
        var state = new CaravanState
        {
            GameId = gameId,
            Players = humans,
            CurrentTurnIndex = 0,
        };

        for (int i = 0; i < allPlayers.Count; i++)
        {
            var deck = BuildDeck(rnd);
            var hand = new List<CaravanCard>();
            for (int k = 0; k < CaravanConstants.HandSize && deck.Count > 0; k++)
            {
                hand.Add(Pop(deck));
            }
            state.PlayerStates.Add(new CaravanPlayerState
            {
                PlayerId = allPlayers[i],
                Hand = hand,
                Deck = deck,
                IsAi = allPlayers[i] == CaravanConstants.AiPlayerId,
            });
        }

        for (int playerIdx = 0; playerIdx < 2; playerIdx++)
        {
            for (int c = 0; c < CaravanConstants.CaravansPerPlayer; c++)
            {
                state.Columns.Add(new CaravanColumn
                {
                    Index = playerIdx * CaravanConstants.CaravansPerPlayer + c,
                    OwnerId = state.PlayerStates[playerIdx].PlayerId,
                });
            }
        }

        return state;
    }

    public MoveResult ValidateMove(CaravanState state, CaravanMove move, string playerId)
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

        if (move.Type == CaravanMoveType.DiscardCaravan)
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
            case CaravanMoveType.DiscardCard:
                return MoveResult.Success;

            case CaravanMoveType.PlayNumber:
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

            case CaravanMoveType.AttachFace:
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
                    if (card.Rank == CaravanRank.Joker && !(slot.Card.Rank == CaravanRank.Ace || slot.Card.IsNumber))
                    {
                        return MoveResult.Failure("Joker can only attach to a number card or ace");
                    }
                    return MoveResult.Success;
                }

            default:
                return MoveResult.Failure("Unknown move type");
        }
    }

    public void ApplyMove(CaravanState state, CaravanMove move, string playerId)
    {
        int playerIdx = IndexOf(state, playerId);
        var ps = state.PlayerStates[playerIdx];
        string desc;

        if (move.Type == CaravanMoveType.DiscardCaravan)
        {
            var col = state.Columns[move.TargetColumnIndex];
            foreach (var slot in col.Slots)
            {
                state.Discard.Add(slot.Card);
                state.Discard.AddRange(slot.Attached);
            }
            desc = $"{ShortName(playerId)} discarded caravan {ColumnLabel(col.Index)}";
            col.Slots.Clear();
            col.Direction = CaravanDirection.Unset;
        }
        else if (move.Type == CaravanMoveType.DiscardCard)
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            state.Discard.Add(card);
            desc = $"{ShortName(playerId)} discarded {card}";
        }
        else if (move.Type == CaravanMoveType.PlayNumber)
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            ApplyNumberPlacement(col, card);
            desc = $"{ShortName(playerId)} placed {card} on caravan {ColumnLabel(col.Index)}";
            DecrementOpening(state, playerIdx);
        }
        else
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            var slot = col.Slots[move.TargetSlotIndex];
            ApplyFaceAttachment(state, col, slot, card);
            desc = $"{ShortName(playerId)} attached {card} to {slot.Card}";
        }

        state.LastMoveDescription = desc;

        // Refill hand
        while (ps.Hand.Count < CaravanConstants.HandSize && ps.Deck.Count > 0)
        {
            ps.Hand.Add(Pop(ps.Deck));
        }

        // Pass turn
        state.CurrentTurnIndex = 1 - state.CurrentTurnIndex;

        // Auto-run AI turns until either it's the human's turn again or the game ends.
        RunAiTurns(state);
    }

    private void RunAiTurns(CaravanState state)
    {
        // Hard cap to avoid infinite loops on edge cases.
        for (int safety = 0; safety < 8; safety++)
        {
            if (state.CurrentTurnIndex < 0 || state.CurrentTurnIndex >= state.PlayerStates.Count)
            {
                return;
            }
            var current = state.PlayerStates[state.CurrentTurnIndex];
            if (!current.IsAi)
            {
                return;
            }
            if (CheckFinished(state))
            {
                return;
            }

            var aiMove = CaravanAi.PickMove(state);
            if (aiMove is null) return;

            var validation = ValidateMove(state, aiMove, current.PlayerId);
            if (!validation.IsSuccess)
            {
                // Fallback: discard the first card so we don't deadlock.
                aiMove = new CaravanMove
                {
                    Type = CaravanMoveType.DiscardCard,
                    HandIndex = 0,
                };
                if (current.Hand.Count == 0) return;
            }

            // Apply AI move inline (don't recurse — we handle our own loop).
            ApplyMoveInternal(state, aiMove, current.PlayerId);

            // Refill AI hand
            while (current.Hand.Count < CaravanConstants.HandSize && current.Deck.Count > 0)
            {
                current.Hand.Add(Pop(current.Deck));
            }

            // Switch turn back
            state.CurrentTurnIndex = 1 - state.CurrentTurnIndex;
        }
    }

    private static void ApplyMoveInternal(CaravanState state, CaravanMove move, string playerId)
    {
        int playerIdx = IndexOf(state, playerId);
        var ps = state.PlayerStates[playerIdx];
        string desc;

        if (move.Type == CaravanMoveType.DiscardCaravan)
        {
            var col = state.Columns[move.TargetColumnIndex];
            foreach (var slot in col.Slots)
            {
                state.Discard.Add(slot.Card);
                state.Discard.AddRange(slot.Attached);
            }
            desc = $"AI discarded caravan {ColumnLabel(col.Index)}";
            col.Slots.Clear();
            col.Direction = CaravanDirection.Unset;
        }
        else if (move.Type == CaravanMoveType.DiscardCard)
        {
            if (move.HandIndex < 0 || move.HandIndex >= ps.Hand.Count) { state.LastMoveDescription = "AI passed"; return; }
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            state.Discard.Add(card);
            desc = $"AI discarded {card}";
        }
        else if (move.Type == CaravanMoveType.PlayNumber)
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            ApplyNumberPlacement(col, card);
            desc = $"AI placed {card} on caravan {ColumnLabel(col.Index)}";
            DecrementOpening(state, playerIdx);
        }
        else
        {
            var card = ps.Hand[move.HandIndex];
            ps.Hand.RemoveAt(move.HandIndex);
            var col = state.Columns[move.TargetColumnIndex];
            var slot = col.Slots[move.TargetSlotIndex];
            ApplyFaceAttachment(state, col, slot, card);
            desc = $"AI attached {card} to {slot.Card}";
        }

        state.LastMoveDescription = desc;
    }

    public bool CheckFinished(CaravanState state)
    {
        if (state.PlayerStates.Count < 2)
        {
            return true;
        }

        // A player wins when they outscore the opponent on at least 2 of the 3 head-to-head caravan pairs,
        // where each caravan is in the "sold" range (21-26). Busted caravans count as 0 for that pairing.
        int p0Wins = 0;
        int p1Wins = 0;
        int decided = 0;
        for (int i = 0; i < CaravanConstants.CaravansPerPlayer; i++)
        {
            var p0Col = state.Columns[i];
            var p1Col = state.Columns[i + CaravanConstants.CaravansPerPlayer];

            bool p0Sold = p0Col.IsSold;
            bool p1Sold = p1Col.IsSold;

            if (!p0Sold && !p1Sold)
            {
                continue;
            }

            decided++;
            int p0Score = p0Sold ? p0Col.Value : 0;
            int p1Score = p1Sold ? p1Col.Value : 0;

            if (p0Score > p1Score)
            {
                p0Wins++;
            }
            else if (p1Score > p0Score)
            {
                p1Wins++;
            }
        }

        if (p0Wins >= 2 || p1Wins >= 2)
        {
            return true;
        }

        // Stalemate: both players out of cards (deck + hand) and no further moves possible.
        bool anyCardsLeft = state.PlayerStates.Any(p => p.Hand.Count > 0 || p.Deck.Count > 0);
        return !anyCardsLeft && decided == CaravanConstants.CaravansPerPlayer;
    }

    public string? DetermineWinner(CaravanState state)
    {
        if (state.PlayerStates.Count < 2)
        {
            return null;
        }

        int p0Wins = 0;
        int p1Wins = 0;
        for (int i = 0; i < CaravanConstants.CaravansPerPlayer; i++)
        {
            var p0Col = state.Columns[i];
            var p1Col = state.Columns[i + CaravanConstants.CaravansPerPlayer];
            int p0 = p0Col.IsSold ? p0Col.Value : 0;
            int p1 = p1Col.IsSold ? p1Col.Value : 0;
            if (p0 > p1) p0Wins++;
            else if (p1 > p0) p1Wins++;
        }

        if (p0Wins > p1Wins) return state.PlayerStates[0].PlayerId;
        if (p1Wins > p0Wins) return state.PlayerStates[1].PlayerId;
        return null;
    }

    public IEnumerable<string> GetExpectedPlayers(CaravanState state)
    {
        if (state.PlayerStates.Count == 0)
        {
            yield break;
        }
        var current = state.PlayerStates[state.CurrentTurnIndex];
        if (!current.IsAi)
        {
            yield return current.PlayerId;
        }
    }

    public bool NeedToFinish(CaravanState state) => false;

    public IReadOnlyDictionary<string, int> GetPlayerScores(CaravanState state)
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
            if (ps.IsAi) continue;
            int points = isDraw ? 1 : (ps.PlayerId == winnerId ? 3 : 0);
            scores[ps.PlayerId] = points;
        }
        return scores;
    }

    private static MoveResult ValidateNumberPlacement(CaravanColumn col, CaravanCard card)
    {
        // Aces are treated as number cards with value 1 and they can open a caravan.
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
            // Second card sets direction; suit constraint is relaxed.
            return MoveResult.Success;
        }

        // Direction must match unless suit matches (suit overrides direction).
        bool ascending = newValue > topValue;
        if (col.Direction == CaravanDirection.Ascending && !ascending && !sameSuit)
        {
            return MoveResult.Failure("Caravan is ascending; play a higher card or match suit to reverse");
        }
        if (col.Direction == CaravanDirection.Descending && ascending && !sameSuit)
        {
            return MoveResult.Failure("Caravan is descending; play a lower card or match suit to reverse");
        }

        return MoveResult.Success;
    }

    private static void ApplyNumberPlacement(CaravanColumn col, CaravanCard card)
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
                col.Direction = ascending ? CaravanDirection.Ascending : CaravanDirection.Descending;
            }
            else if (sameSuit)
            {
                if (col.Direction == CaravanDirection.Ascending && !ascending)
                {
                    col.Direction = CaravanDirection.Descending;
                }
                else if (col.Direction == CaravanDirection.Descending && ascending)
                {
                    col.Direction = CaravanDirection.Ascending;
                }
            }
        }

        col.Slots.Add(new CaravanColumnSlot { Card = card });
    }

    private static void ApplyFaceAttachment(CaravanState state, CaravanColumn col, CaravanColumnSlot slot, CaravanCard card)
    {
        switch (card.Rank)
        {
            case CaravanRank.Jack:
                {
                    state.Discard.Add(slot.Card);
                    state.Discard.AddRange(slot.Attached);
                    state.Discard.Add(card);
                    col.Slots.Remove(slot);
                    if (col.Slots.Count < 2)
                    {
                        col.Direction = CaravanDirection.Unset;
                    }
                    break;
                }
            case CaravanRank.Queen:
                {
                    slot.Attached.Add(card);
                    if (col.Slots.Count >= 2 && ReferenceEquals(col.Top, slot))
                    {
                        col.Direction = col.Direction switch
                        {
                            CaravanDirection.Ascending => CaravanDirection.Descending,
                            CaravanDirection.Descending => CaravanDirection.Ascending,
                            _ => CaravanDirection.Unset,
                        };
                    }
                    break;
                }
            case CaravanRank.King:
                {
                    slot.Attached.Add(card);
                    break;
                }
            case CaravanRank.Joker:
                {
                    slot.Attached.Add(card);
                    if (slot.Card.Rank == CaravanRank.Ace)
                    {
                        // Remove all cards of same suit (except attached) across all columns
                        RemoveBySuit(state, slot, slot.Card.Suit);
                    }
                    else
                    {
                        // Number card: remove all cards of same numeric value (except attached and the host slot)
                        RemoveByValue(state, slot, slot.Card.NumericValue);
                    }
                    break;
                }
        }
    }

    private static void RemoveBySuit(CaravanState state, CaravanColumnSlot host, CaravanSuit suit)
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
                col.Direction = CaravanDirection.Unset;
            }
        }
    }

    private static void RemoveByValue(CaravanState state, CaravanColumnSlot host, int value)
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
                col.Direction = CaravanDirection.Unset;
            }
        }
    }

    private static void DecrementOpening(CaravanState state, int playerIdx)
    {
        if (playerIdx == 0 && state.OpeningCardsRemainingP0 > 0)
            state.OpeningCardsRemainingP0--;
        else if (playerIdx == 1 && state.OpeningCardsRemainingP1 > 0)
            state.OpeningCardsRemainingP1--;
    }

    private static int IndexOf(CaravanState state, string playerId)
    {
        for (int i = 0; i < state.PlayerStates.Count; i++)
        {
            if (state.PlayerStates[i].PlayerId == playerId)
                return i;
        }
        return -1;
    }

    private static List<CaravanCard> BuildDeck(Random rnd)
    {
        var cards = new List<CaravanCard>();
        for (int s = 0; s < 4; s++)
        {
            for (int r = (int)CaravanRank.Ace; r <= (int)CaravanRank.King; r++)
            {
                cards.Add(new CaravanCard { Suit = (CaravanSuit)s, Rank = (CaravanRank)r });
            }
        }
        // Add 2 jokers (one per "color")
        cards.Add(new CaravanCard { Suit = CaravanSuit.Hearts, Rank = CaravanRank.Joker });
        cards.Add(new CaravanCard { Suit = CaravanSuit.Spades, Rank = CaravanRank.Joker });

        // Shuffle and trim to a reasonable size (>= 30)
        Shuffle(cards, rnd);
        var deck = cards.Take(30).ToList();
        return deck;
    }

    private static void Shuffle(List<CaravanCard> list, Random rnd)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static CaravanCard Pop(List<CaravanCard> deck)
    {
        var card = deck[^1];
        deck.RemoveAt(deck.Count - 1);
        return card;
    }

    private static string ColumnLabel(int index)
    {
        // Player 0 caravans => A,B,C; Player 1 caravans => D,E,F
        return ((char)('A' + index)).ToString();
    }

    private static string ShortName(string playerId)
    {
        if (playerId == CaravanConstants.AiPlayerId) return "AI";
        return playerId.Length <= 8 ? playerId : playerId.Substring(0, 8);
    }
}
