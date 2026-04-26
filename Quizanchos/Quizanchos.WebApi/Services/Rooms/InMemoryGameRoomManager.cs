using System.Collections.Concurrent;
using Quizanchos.WebApi.Models.Rooms;

namespace Quizanchos.WebApi.Services.Rooms;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IGameRoomManager"/>.
/// Suitable for single-server deployments. Replace with a distributed store
/// (e.g., Redis) for multi-instance scenarios.
/// </summary>
public class InMemoryGameRoomManager : IGameRoomManager
{
    private readonly ConcurrentDictionary<Guid, GameRoom> _rooms = new();

    public GameRoom CreateRoom(
        Guid roomId,
        int minigameType,
        string hostPlayerId,
        int maxPlayers,
        List<GameRoomTeam> teams,
        Dictionary<string, object>? gameParameters)
    {
        var room = new GameRoom(roomId, minigameType, hostPlayerId, maxPlayers, teams, gameParameters);
        if (!_rooms.TryAdd(roomId, room))
            throw new InvalidOperationException("Room with this ID already exists.");

        return room;
    }

    public GameRoom? GetRoom(Guid roomId) => _rooms.GetValueOrDefault(roomId);

    public bool RemoveRoom(Guid roomId) => _rooms.TryRemove(roomId, out _);

    public IReadOnlyList<GameRoom> GetAvailableRooms(int? minigameType = null)
    {
        var query = _rooms.Values.Where(r =>
            r.Status == GameRoomStatus.WaitingForPlayers && !r.IsJoinExpired);

        if (minigameType.HasValue)
            query = query.Where(r => r.MinigameType == minigameType.Value);

        return query.ToList();
    }

    public GameRoom? GetRoomByPlayerId(string playerId) =>
        _rooms.Values.FirstOrDefault(r =>
            r.ContainsPlayer(playerId) &&
            r.Status is GameRoomStatus.WaitingForPlayers or GameRoomStatus.Launching);
}
