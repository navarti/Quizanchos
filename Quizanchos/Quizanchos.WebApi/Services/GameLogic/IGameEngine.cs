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

    /// <summary>
    /// Returns the score points earned by each player upon game finish.
    /// This allows minigames to define custom scoring (winners, draws, participation, etc).
    /// </summary>
    /// <returns>Dictionary mapping player ID to earned points. Empty dict if no points to award.</returns>
    IReadOnlyDictionary<string, int> GetPlayerScores();
}
