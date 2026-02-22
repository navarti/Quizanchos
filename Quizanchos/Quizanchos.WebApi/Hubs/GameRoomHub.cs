using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Quizanchos.WebApi.Constants;
using System.Security.Claims;

namespace Quizanchos.WebApi.Hubs;

/// <summary>
/// SignalR hub for real-time game room communication.
/// Clients subscribe to a room group to receive lobby updates, team changes, and game launch events.
/// </summary>
[Authorize(AppRole.User)]
public class GameRoomHub : Hub
{
    private readonly ILogger<GameRoomHub> _logger;

    public GameRoomHub(ILogger<GameRoomHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to real-time updates for a specific room.
    /// </summary>
    public async Task SubscribeToRoom(Guid roomId)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        string groupName = GetGroupName(roomId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Player {PlayerId} subscribed to room {RoomId}", userId, roomId);
    }

    /// <summary>
    /// Unsubscribe from real-time updates for a specific room.
    /// </summary>
    public async Task UnsubscribeFromRoom(Guid roomId)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        string groupName = GetGroupName(roomId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Player {PlayerId} unsubscribed from room {RoomId}", userId, roomId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from room hub: {ConnectionId}, Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "normal");

        await base.OnDisconnectedAsync(exception);
    }

    internal static string GetGroupName(Guid roomId) => $"room-{roomId}";
}
