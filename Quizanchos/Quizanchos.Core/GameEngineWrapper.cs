namespace Quizanchos.Core;

/// <summary>
/// Wrapper that adapts a strongly-typed GameEngine to the generic IGameEngine interface.
/// This allows different minigame implementations with their own state and move types
/// to work with the generic game logic factory.
/// </summary>
public class GameEngineWrapper<TState, TMove> : IGameEngine
    where TState : IGameState
{
    private readonly GameEngine<TState, TMove> _engine;

    public GameEngineWrapper(GameEngine<TState, TMove> engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
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

    public bool NeedToFinish()
    {
        return _engine.NeedToFinish();
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores()
    {
        return _engine.GetPlayerScores();
    }
}
