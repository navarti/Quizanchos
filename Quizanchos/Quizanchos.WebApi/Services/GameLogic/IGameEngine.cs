using Quizanchos.Core;

namespace Quizanchos.WebApi.Services.GameLogic;

public interface IGameEngine
{
    Guid GameId { get; }
    IReadOnlyList<Guid> Players { get; }
    bool IsFinished { get; }
    Guid? Winner { get; }
    
    MoveResult MakeMove(Guid playerId, GameMove move);
    IGameState GetState();
    IEnumerable<Guid> GetExpectedPlayers();
}
