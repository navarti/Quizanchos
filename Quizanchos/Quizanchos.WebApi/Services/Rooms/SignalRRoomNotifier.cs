using Microsoft.AspNetCore.SignalR;
using Quizanchos.Core;
using Quizanchos.WebApi.Hubs;

namespace Quizanchos.WebApi.Services.Rooms;

/// <summary>
/// SignalR-based implementation of <see cref="IRoomNotifier"/>.
/// Pushes room events to all clients in the SignalR group for a given room.
/// </summary>
public class SignalRRoomNotifier : IRoomNotifier
{
    private readonly IHubContext<GameRoomHub> _hubContext;
    private readonly ILogger<SignalRRoomNotifier> _logger;

    public SignalRRoomNotifier(IHubContext<GameRoomHub> hubContext, ILogger<SignalRRoomNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyPlayerJoinedRoom(Guid roomId, string playerId, int teamIndex)
    {
        string groupName = GameRoomHub.GetGroupName(roomId);
        _logger.LogInformation("Broadcasting player {PlayerId} joined room {RoomId} team {TeamIndex}",
            playerId, roomId, teamIndex);

        await _hubContext.Clients.Group(groupName).SendAsync("PlayerJoinedRoom", new
        {
            RoomId = roomId,
            PlayerId = playerId,
            TeamIndex = teamIndex
        });
    }

    public async Task NotifyPlayerLeftRoom(Guid roomId, string playerId)
    {
        string groupName = GameRoomHub.GetGroupName(roomId);
        _logger.LogInformation("Broadcasting player {PlayerId} left room {RoomId}", playerId, roomId);

        await _hubContext.Clients.Group(groupName).SendAsync("PlayerLeftRoom", new
        {
            RoomId = roomId,
            PlayerId = playerId
        });
    }

    public async Task NotifyRoomUpdated(Guid roomId, object roomSnapshot)
    {
        string groupName = GameRoomHub.GetGroupName(roomId);
        _logger.LogInformation("Broadcasting room update for room {RoomId}", roomId);

        await _hubContext.Clients.Group(groupName).SendAsync("RoomUpdated", roomSnapshot);
    }

    public async Task NotifyGameStarting(Guid roomId, Guid gameId)
    {
        string groupName = GameRoomHub.GetGroupName(roomId);
        _logger.LogInformation("Broadcasting game starting for room {RoomId}, GameId {GameId}", roomId, gameId);

        await _hubContext.Clients.Group(groupName).SendAsync("GameStarting", new
        {
            RoomId = roomId,
            GameId = gameId
        });
    }

    public async Task NotifyRoomClosed(Guid roomId, string reason)
    {
        string groupName = GameRoomHub.GetGroupName(roomId);
        _logger.LogInformation("Broadcasting room {RoomId} closed: {Reason}", roomId, reason);

        await _hubContext.Clients.Group(groupName).SendAsync("RoomClosed", new
        {
            RoomId = roomId,
            Reason = reason
        });
    }
}
