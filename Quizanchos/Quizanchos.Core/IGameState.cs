using Quizanchos.Common.Enums;

namespace Quizanchos.Core;

public interface IGameState
{
    Guid GameId { get; }
    MinigameType MinigameType { get; }
    IReadOnlyList<string> Players { get; }

    bool IsFinished { get; set; }
    string? Winner { get; set; }
}
