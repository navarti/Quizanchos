namespace Quizanchos.Core;

public interface IGameRules<TState, TMove>
    where TState : IGameState
{
    void ApplyMove(TState state, TMove move, string playerId);
    bool CheckFinished(TState state);
    string? DetermineWinner(TState state);
    IEnumerable<string> GetExpectedPlayers(TState state);
    bool NeedToFinish(TState state);

    /// <summary>
    /// Returns the score points earned by each player upon game finish.
    /// Allows minigames to define custom scoring (winners, draws, participation, etc).
    /// </summary>
    /// <returns>Dictionary mapping player ID to earned points. Empty dict if no points to award.</returns>
    IReadOnlyDictionary<string, int> GetPlayerScores(TState state)
    {
        // Default implementation: if there's a winner, they get 10 points
        // Minigames can override this for custom scoring logic (draws, participation, etc.)
        var winner = DetermineWinner(state);
        if (!string.IsNullOrEmpty(winner))
        {
            return new Dictionary<string, int> { { winner, 10 } };
        }
        return new Dictionary<string, int>();
    }
}
