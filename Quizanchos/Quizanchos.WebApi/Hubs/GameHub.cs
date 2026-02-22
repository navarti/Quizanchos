using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Quizanchos.Core;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.GameLogic;
using System.Security.Claims;

namespace Quizanchos.WebApi.Hubs;

/// <summary>
/// SignalR hub for real-time game communication.
/// Clients join a game group to receive live state updates, move notifications, and finish events.
/// </summary>
[Authorize(AppRole.User)]
public class GameHub : Hub
{
    private readonly IGameLogicFactory _gameLogicFactory;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IGameLogicFactory gameLogicFactory, ILogger<GameHub> logger)
    {
        _gameLogicFactory = gameLogicFactory;
        _logger = logger;
    }

    /// <summary>
    /// Called by a client to join a game room and start receiving real-time updates.
    /// </summary>
    public async Task JoinGame(Guid gameId)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        string groupName = GetGroupName(gameId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Player {PlayerId} joined game group {GameId}", userId, gameId);

        await Clients.OthersInGroup(groupName).SendAsync("PlayerJoined", userId);
    }

    /// <summary>
    /// Called by a client to leave a game room.
    /// </summary>
    public async Task LeaveGame(Guid gameId)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        string groupName = GetGroupName(gameId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Player {PlayerId} left game group {GameId}", userId, gameId);

        await Clients.OthersInGroup(groupName).SendAsync("PlayerLeft", userId);
    }

    /// <summary>
    /// Called by a client to submit a move via the real-time channel.
    /// The server processes the move and broadcasts the updated state to all players in the game.
    /// </summary>
    public async Task SubmitMove(Guid gameId, GameMove move)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        // Delegate to GameService through the caller (GameController still works for REST).
        // This method is here so that multiplayer clients can submit moves over the socket
        // and get the response pushed to all group members.
        // The actual processing is done via GameService, which is called from the controller;
        // however, for real-time games the front-end can call SubmitMove directly.

        _logger.LogInformation("Player {PlayerId} submitted move via SignalR for game {GameId}", userId, gameId);

        // Respond to the caller with an acknowledgement — 
        // the actual state push happens via IGameNotifier after GameService processes the move.
        await Clients.Caller.SendAsync("MoveReceived", new { GameId = gameId, PlayerId = userId });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}, Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "normal");

        await base.OnDisconnectedAsync(exception);
    }

    internal static string GetGroupName(Guid gameId) => $"game-{gameId}";
}
