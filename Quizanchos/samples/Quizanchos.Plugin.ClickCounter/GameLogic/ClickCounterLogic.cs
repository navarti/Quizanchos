using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.Plugin.ClickCounter.GameLogic;

public sealed class ClickCounterLogic : IGameLogic<ClickCounterState, ClickCounterMove>
{
    private readonly int _target;

    public ClickCounterLogic(int target = 10)
    {
        _target = target;
    }

    public ClickCounterState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        return new ClickCounterState
        {
            GameId = gameId,
            Players = players.ToList(),
            ClickCount = 0,
            Target = _target,
        };
    }

    public MoveResult ValidateMove(ClickCounterState state, ClickCounterMove move, string playerId)
    {
        if (!state.Players.Contains(playerId))
        {
            return MoveResult.Failure("Player not in game");
        }
        return MoveResult.Success;
    }

    public void ApplyMove(ClickCounterState state, ClickCounterMove move, string playerId)
    {
        state.ClickCount++;
    }

    public bool CheckFinished(ClickCounterState state) => state.ClickCount >= state.Target;

    public string? DetermineWinner(ClickCounterState state)
        => state.Players.Count > 0 ? state.Players[0] : null;

    public IEnumerable<string> GetExpectedPlayers(ClickCounterState state) => state.Players;

    public bool NeedToFinish(ClickCounterState state) => false;

    public IReadOnlyDictionary<string, int> GetPlayerScores(ClickCounterState state)
    {
        if (state.Players.Count == 0)
        {
            return new Dictionary<string, int>();
        }
        return new Dictionary<string, int> { { state.Players[0], state.ClickCount } };
    }
}
