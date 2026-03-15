using System.Collections.Immutable;

namespace Quizanchos.Core;

public class GameEngine<TState, TMove>
    where TState : IGameState
{
    private readonly IGameLogic<TState, TMove> _logic;
    public TState State { get; }

    private readonly HashSet<string> _movesReceived = new();

    public GameEngine(
        IGameLogic<TState, TMove> logic,
        Guid gameId,
        ImmutableArray<string> players)
    {
        _logic = logic;
        State = _logic.CreateInitialState(gameId, players);
    }

    public GameEngine(
        IGameLogic<TState, TMove> logic,
        TState existingState)
    {
        _logic = logic;
        State = existingState;
    }

    public MoveResult MakeMove(string playerId, TMove move)
    {
        if (State.IsFinished)
            return MoveResult.Failure("Game is finished");

        var expected = _logic.GetExpectedPlayers(State);
        if (!expected.Contains(playerId))
            return MoveResult.Failure("Player can not commit now");

        MoveResult moveValidationResult = _logic.ValidateMove(State, move, playerId);
        if (!moveValidationResult.IsSuccess)
            return moveValidationResult;

        _logic.ApplyMove(State, move, playerId);
        _movesReceived.Add(playerId);

        // Якщо гра очікувала всіх — чекати
        if (_movesReceived.Count < expected.Count())
            return MoveResult.Success;

        // Усі гравці цього раунду зробили свій хід
        _movesReceived.Clear();

        if (_logic.CheckFinished(State))
        {
            State.IsFinished = true;
            State.Winner = _logic.DetermineWinner(State);
        }

        return MoveResult.Success;
    }

    public bool NeedToFinish()
    {
        if (State.IsFinished)
            return false;

        return _logic.NeedToFinish(State);
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores()
    {
        return _logic.GetPlayerScores(State);
    }
}
