using Quizanchos.Core;

namespace Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

public enum CaravanMpMoveType
{
    PlayNumber = 0,
    AttachFace = 1,
    DiscardCard = 2,
    DiscardCaravan = 3,
}

public sealed record CaravanMpMove : GameMove
{
    public CaravanMpMoveType Type { get; init; }
    public int HandIndex { get; init; } = -1;
    public int TargetColumnIndex { get; init; } = -1;
    public int TargetSlotIndex { get; init; } = -1;
}
