using Quizanchos.WebApi.Models.Rooms;

namespace Quizanchos.WebApi.Services.Rooms;

/// <summary>
/// Manages the storage and retrieval of game rooms.
/// Implementations decide where rooms are stored (in-memory, Redis, DB, etc.).
/// </summary>
public interface IGameRoomManager
{
    GameRoom CreateRoom(
        Guid roomId,
        int minigameType,
        string hostPlayerId,
        int maxPlayers,
        List<GameRoomTeam> teams,
        Dictionary<string, object>? gameParameters);

    GameRoom? GetRoom(Guid roomId);

    bool RemoveRoom(Guid roomId);

    IReadOnlyList<GameRoom> GetAvailableRooms(int? minigameType = null);

    GameRoom? GetRoomByPlayerId(string playerId);
}
