namespace Quizanchos.Plugin.Caravan.GameLogic;

/// <summary>
/// Picks a heuristic move for the AI player (player index 1). The strategy is intentionally
/// simple: prefer to bring own caravans into the 21-26 sell range, prefer to attack opponent
/// caravans that are in or close to the sell range, otherwise discard a useless card.
/// </summary>
internal static class CaravanAi
{
    public static CaravanMove? PickMove(CaravanState state)
    {
        if (state.CurrentTurnIndex != 1)
        {
            return null;
        }
        var ps = state.PlayerStates[1];
        if (ps.Hand.Count == 0)
        {
            return null;
        }

        var logic = new CaravanLogic();

        // Opening phase: must place a number card or ace on each empty own caravan.
        if (CaravanLogic.IsInOpeningPhase(state, 1))
        {
            return PickOpeningMove(state, ps);
        }

        // Phase 1: Try to play a number card on a sub-21 own caravan, picking the move that
        // either sells the caravan or gets it closer without busting.
        for (int handIdx = 0; handIdx < ps.Hand.Count; handIdx++)
        {
            var card = ps.Hand[handIdx];
            if (!card.IsNumber) continue;

            int bestColumn = -1;
            int bestScore = int.MinValue;

            for (int colIdx = 3; colIdx < 6; colIdx++)
            {
                var col = state.Columns[colIdx];
                int currentValue = col.Value;
                int projected = currentValue + card.NumericValue;
                if (projected > CaravanConstants.CaravanSellMax) continue;
                var probe = new CaravanMove
                {
                    Type = CaravanMoveType.PlayNumber,
                    HandIndex = handIdx,
                    TargetColumnIndex = colIdx,
                };
                var validation = logic.ValidateMove(state, probe, ps.PlayerId);
                if (!validation.IsSuccess) continue;

                int score = projected;
                if (projected >= CaravanConstants.CaravanSellMin) score += 100;
                if (col.Slots.Count == 0) score -= 5; // small penalty for opening if alternatives exist
                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = colIdx;
                }
            }

            if (bestColumn >= 0)
            {
                return new CaravanMove
                {
                    Type = CaravanMoveType.PlayNumber,
                    HandIndex = handIdx,
                    TargetColumnIndex = bestColumn,
                };
            }
        }

        // Phase 2: Try to attack the opponent's strong caravans with Jack/Joker, or boost own with King/Queen.
        for (int handIdx = 0; handIdx < ps.Hand.Count; handIdx++)
        {
            var card = ps.Hand[handIdx];
            if (!card.IsFace) continue;

            if (card.Rank == CaravanRank.Jack || card.Rank == CaravanRank.Joker)
            {
                // Target opponent (columns 0-2) — pick the highest-value slot we can hit.
                int bestCol = -1;
                int bestSlot = -1;
                int bestVal = -1;
                for (int colIdx = 0; colIdx < 3; colIdx++)
                {
                    var col = state.Columns[colIdx];
                    for (int s = 0; s < col.Slots.Count; s++)
                    {
                        var slot = col.Slots[s];
                        if (card.Rank == CaravanRank.Joker
                            && !(slot.Card.Rank == CaravanRank.Ace || slot.Card.IsNumber))
                            continue;
                        int v = slot.EffectiveValue;
                        if (v > bestVal)
                        {
                            bestVal = v;
                            bestCol = colIdx;
                            bestSlot = s;
                        }
                    }
                }
                if (bestCol >= 0)
                {
                    return new CaravanMove
                    {
                        Type = CaravanMoveType.AttachFace,
                        HandIndex = handIdx,
                        TargetColumnIndex = bestCol,
                        TargetSlotIndex = bestSlot,
                    };
                }
            }
            else if (card.Rank == CaravanRank.King)
            {
                // Boost an own caravan that needs more value.
                int bestCol = -1;
                int bestSlot = -1;
                int bestProjected = -1;
                for (int colIdx = 3; colIdx < 6; colIdx++)
                {
                    var col = state.Columns[colIdx];
                    int currentValue = col.Value;
                    if (col.Slots.Count == 0) continue;
                    var topSlot = col.Top!;
                    int projected = currentValue + topSlot.EffectiveValue;
                    if (projected > CaravanConstants.CaravanSellMax) continue;
                    if (projected > bestProjected)
                    {
                        bestProjected = projected;
                        bestCol = colIdx;
                        bestSlot = col.Slots.Count - 1;
                    }
                }
                if (bestCol >= 0)
                {
                    return new CaravanMove
                    {
                        Type = CaravanMoveType.AttachFace,
                        HandIndex = handIdx,
                        TargetColumnIndex = bestCol,
                        TargetSlotIndex = bestSlot,
                    };
                }
            }
            else if (card.Rank == CaravanRank.Queen)
            {
                // Use Queen on own caravan top to flip direction if stuck.
                for (int colIdx = 3; colIdx < 6; colIdx++)
                {
                    var col = state.Columns[colIdx];
                    if (col.Slots.Count >= 2)
                    {
                        return new CaravanMove
                        {
                            Type = CaravanMoveType.AttachFace,
                            HandIndex = handIdx,
                            TargetColumnIndex = colIdx,
                            TargetSlotIndex = col.Slots.Count - 1,
                        };
                    }
                }
            }
        }

        // Phase 3: Discard the worst-fitting card.
        int discardIdx = 0;
        for (int i = 1; i < ps.Hand.Count; i++)
        {
            // Prefer to discard face cards we can't use, then high-value numbers we can't place.
            if (ps.Hand[i].IsFace && !ps.Hand[discardIdx].IsFace)
                discardIdx = i;
        }
        return new CaravanMove
        {
            Type = CaravanMoveType.DiscardCard,
            HandIndex = discardIdx,
        };
    }

    private static CaravanMove? PickOpeningMove(CaravanState state, CaravanPlayerState ps)
    {
        // Find the first empty owned caravan (AI is player index 1, so cols 3-5).
        int targetCol = -1;
        for (int colIdx = 3; colIdx < 3 + CaravanConstants.CaravansPerPlayer; colIdx++)
        {
            if (state.Columns[colIdx].Slots.Count == 0)
            {
                targetCol = colIdx;
                break;
            }
        }

        // Prefer the highest-value number card so the opening caravan starts closer to the 21-26 range.
        int bestHandIdx = -1;
        int bestValue = -1;
        for (int i = 0; i < ps.Hand.Count; i++)
        {
            var card = ps.Hand[i];
            if (!card.IsNumber) continue;
            if (card.NumericValue > bestValue)
            {
                bestValue = card.NumericValue;
                bestHandIdx = i;
            }
        }

        if (targetCol < 0 || bestHandIdx < 0)
        {
            // No legal opening move (no empty caravan or no number card in hand) — discard so we don't deadlock.
            return new CaravanMove
            {
                Type = CaravanMoveType.DiscardCard,
                HandIndex = 0,
            };
        }

        return new CaravanMove
        {
            Type = CaravanMoveType.PlayNumber,
            HandIndex = bestHandIdx,
            TargetColumnIndex = targetCol,
        };
    }
}
