using Quizanchos.Core;

namespace Quizanchos.WebApi.Services.GameLogic;

public interface IGameEngine
{
    Guid GameId { get; }
    IReadOnlyList<string> Players { get; }
    bool IsFinished { get; }
    string? Winner { get; }
    
    MoveResult MakeMove(string playerId, GameMove move);
    IGameState GetState();
    bool NeedToFinish();
}
