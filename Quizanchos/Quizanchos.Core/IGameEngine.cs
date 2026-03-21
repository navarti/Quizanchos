namespace Quizanchos.Core;

/// <summary>
/// Interface for a game engine that handles game logic and state management.
/// All minigame engines must implement this interface.
/// </summary>
public interface IGameEngine
{
    /// <summary>
    /// Unique identifier for the game instance
    /// </summary>
    Guid GameId { get; }

    /// <summary>
    /// List of player IDs in the game
    /// </summary>
    IReadOnlyList<string> Players { get; }

    /// <summary>
    /// Whether the game has finished
    /// </summary>
    bool IsFinished { get; }

    /// <summary>
    /// ID of the winning player, if any
    /// </summary>
    string? Winner { get; }

    /// <summary>
    /// Make a move in the game
    /// </summary>
    MoveResult MakeMove(string playerId, GameMove move);

    /// <summary>
    /// Get the current game state
    /// </summary>
    IGameState GetState();

    /// <summary>
    /// Check if the game needs to finish (e.g., no more moves possible)
    /// </summary>
    bool NeedToFinish();

    /// <summary>
    /// Returns the score points earned by each player upon game finish.
    /// This allows minigames to define custom scoring (winners, draws, participation, etc).
    /// </summary>
    /// <returns>Dictionary mapping player ID to earned points. Empty dict if no points to award.</returns>
    IReadOnlyDictionary<string, int> GetPlayerScores();
}
