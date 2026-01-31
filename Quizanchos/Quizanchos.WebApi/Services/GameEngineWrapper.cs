using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.WebApi.Services;

public class GameEngineWrapper<TState, TMove> : IGameEngine
    where TState : IGameState
{
    private readonly GameEngine<TState, TMove> _engine;

    public GameEngineWrapper(GameEngine<TState, TMove> engine)
    {
        _engine = engine;
    }

    public Guid GameId => _engine.State.GameId;
    public IReadOnlyList<Guid> Players => _engine.State.Players;
    public bool IsFinished => _engine.State.IsFinished;
    public Guid? Winner => _engine.State.Winner;

    public MoveResult MakeMove(Guid playerId, GameMove move)
    {
        if (move is not TMove typedMove)
        {
            return MoveResult.Failure($"Invalid move type. Expected {typeof(TMove).Name}");
        }
        
        return _engine.MakeMove(playerId, typedMove);
    }

    public object GetState()
    {
        return _engine.State;
    }

    public IEnumerable<Guid> GetExpectedPlayers()
    {
        return _engine.State.Players.Where(p => !_engine.State.IsFinished);
    }

    public GameEngine<TState, TMove> GetTypedEngine()
    {
        return _engine;
    }
}
