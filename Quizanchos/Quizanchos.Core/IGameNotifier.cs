namespace Quizanchos.Core;

/// <summary>
/// Abstraction for pushing real-time game state updates to connected players.
/// Implementations may use SignalR, WebSockets, or any other transport.
/// </summary>
public interface IGameNotifier
{
    /// <summary>
    /// Notify all players in a game that the state has changed.
    /// </summary>
    Task NotifyStateChanged(Guid gameId, IGameState state);

    /// <summary>
    /// Notify all players in a game that a specific player made a move.
    /// </summary>
    Task NotifyMoveMade(Guid gameId, string playerId, IGameState state);

    /// <summary>
    /// Notify all players in a game that the game has finished.
    /// </summary>
    Task NotifyGameFinished(Guid gameId, IGameState state, string? winner);

    /// <summary>
    /// Notify all players in a game that a player has joined.
    /// </summary>
    Task NotifyPlayerJoined(Guid gameId, string playerId);

    /// <summary>
    /// Notify all players in a game that a player has left.
    /// </summary>
    Task NotifyPlayerLeft(Guid gameId, string playerId);
}
