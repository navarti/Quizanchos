namespace Quizanchos.Core;

/// <summary>
/// Host-provided persistence for plugin game state. Plugins use this instead of touching
/// host domain types directly. The host implementation manages the underlying session row
/// and the per-game state JSON blob; the plugin only knows how to (de)serialize its own
/// <see cref="IGameState"/>.
///
/// Lifetime: scoped (matches the host's underlying repositories).
/// </summary>
public interface IGameStatePersistence
{
    /// <summary>
    /// Create the session and an initial state row. Called once per game when the engine
    /// is first instantiated.
    /// </summary>
    Task CreateAsync(Guid gameId, int minigameTypeId, IReadOnlyList<string> playerIds, string stateJson);

    /// <summary>
    /// Load the persisted state for a game, or null if the game does not exist.
    /// </summary>
    Task<LoadedState?> LoadAsync(Guid gameId);

    /// <summary>
    /// Replace the persisted state JSON for an existing game.
    /// </summary>
    Task UpdateAsync(Guid gameId, string stateJson);
}
