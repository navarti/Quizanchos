using Quizanchos.Core;

namespace Quizanchos.Plugin.Caravan.GameLogic;

public enum CaravanMoveType
{
    PlayNumber = 0,
    AttachFace = 1,
    DiscardCard = 2,
    DiscardCaravan = 3,
}

/// <summary>
/// A move in Caravan.
/// PlayNumber: place a number card (rank 2-10) onto a caravan column. Aces also use this branch.
///   - HandIndex: position in the hand
///   - TargetColumnIndex: 0-2 (own caravans) or 3-5 (opponent caravans, only allowed for Queen suit/dir change indirectly via attach)
/// AttachFace: attach a face card (J/Q/K/Joker) to an existing card.
///   - HandIndex: position in the hand
///   - TargetColumnIndex: 0-5 (any caravan)
///   - TargetSlotIndex: index of the base card in that column
/// DiscardCard: drop a card from hand into the discard pile (skips turn effectively).
///   - HandIndex: position in the hand
/// DiscardCaravan: clear all cards from one of the player's own caravans.
///   - TargetColumnIndex: 0-2 (own only)
/// </summary>
public sealed record CaravanMove : GameMove
{
    public CaravanMoveType Type { get; init; }
    public int HandIndex { get; init; } = -1;
    public int TargetColumnIndex { get; init; } = -1;
    public int TargetSlotIndex { get; init; } = -1;
}
