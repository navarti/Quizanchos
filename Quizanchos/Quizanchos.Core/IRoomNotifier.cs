namespace Quizanchos.Core;

/// <summary>
/// Abstraction for pushing real-time game room updates to connected players.
/// Implementations may use SignalR, WebSockets, or any other transport.
/// </summary>
public interface IRoomNotifier
{
    /// <summary>
    /// Notify all participants that a player joined a room team.
    /// </summary>
    Task NotifyPlayerJoinedRoom(Guid roomId, string playerId, int teamIndex);

    /// <summary>
    /// Notify all participants that a player left the room.
    /// </summary>
    Task NotifyPlayerLeftRoom(Guid roomId, string playerId);

    /// <summary>
    /// Broadcast the current room snapshot to all participants.
    /// </summary>
    Task NotifyRoomUpdated(Guid roomId, object roomSnapshot);

    /// <summary>
    /// Notify all participants that the game is about to start.
    /// </summary>
    Task NotifyGameStarting(Guid roomId, Guid gameId);

    /// <summary>
    /// Notify all participants that the room has been closed.
    /// </summary>
    Task NotifyRoomClosed(Guid roomId, string reason);
}
