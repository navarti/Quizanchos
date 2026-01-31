using Quizanchos.Core;

namespace Quizanchos.WebApi.Services;

public interface IGameEngine
{
    Guid GameId { get; }
    IReadOnlyList<Guid> Players { get; }
    bool IsFinished { get; }
    Guid? Winner { get; }
    
    MoveResult MakeMove(Guid playerId, GameMove move);
    object GetState();
    IEnumerable<Guid> GetExpectedPlayers();
}
