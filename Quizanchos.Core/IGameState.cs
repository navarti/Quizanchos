namespace Quizanchos.Core;

public interface IGameState
{
    Guid GameId { get; }
    IReadOnlyList<Guid> Players { get; }

    bool IsFinished { get; set; }
    Guid? Winner { get; set; }
}
