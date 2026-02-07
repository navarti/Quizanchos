using Quizanchos.Core;

namespace Quizanchos.WebApi.Services.GameLogic;

public class GameEngineWrapper<TState, TMove> : IGameEngine
    where TState : IGameState
{
    private readonly GameEngine<TState, TMove> _engine;

    public GameEngineWrapper(GameEngine<TState, TMove> engine)
    {
        _engine = engine;
    }

    public Guid GameId => _engine.State.GameId;
    public IReadOnlyList<string> Players => _engine.State.Players;
    public bool IsFinished => _engine.State.IsFinished;
    public string? Winner => _engine.State.Winner;

    public MoveResult MakeMove(string playerId, GameMove move)
    {
        if (move is not TMove typedMove)
        {
            return MoveResult.Failure($"Invalid move type. Expected {typeof(TMove).Name}");
        }
        
        return _engine.MakeMove(playerId, typedMove);
    }

    public IGameState GetState()
    {
        return _engine.State;
    }
}
