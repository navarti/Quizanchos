using Quizanchos.Common.Enums;

namespace Quizanchos.Core;

public interface IGameState
{
    Guid GameId { get; }
    MinigameType MinigameType { get; }
    IReadOnlyList<Guid> Players { get; }

    bool IsFinished { get; set; }
    Guid? Winner { get; set; }
}
