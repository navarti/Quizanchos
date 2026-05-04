using Quizanchos.Core;

namespace Quizanchos.Plugin.ClickCounter.GameLogic;

public sealed class ClickCounterState : IGameState
{
    public int MinigameType => ClickCounterConstants.MinigameTypeId;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public int ClickCount { get; set; }
    public int Target { get; set; } = 10;
}

internal static class ClickCounterConstants
{
    public const int MinigameTypeId = 1000;
    public const string GameKey = "ClickCounter";
}
