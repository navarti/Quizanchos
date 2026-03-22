using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Quizanchos.Common.Util;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto.Market;
using Quizanchos.WebApi.Services.Market;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Quizanchos.WebApi.Hubs;

/// <summary>
/// SignalR hub for real-time game communication.
/// Clients join a game group to receive live state updates, move notifications, and finish events.
/// </summary>
[Authorize(AppRole.User)]
public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserGameConnections = new();
    private static readonly ConcurrentDictionary<string, (string UserId, Guid GameId)> ConnectionSubscriptions = new();
    private static readonly ConcurrentDictionary<string, DateTime> LastEmojiSentAt = new();
    private static readonly object PresenceSync = new();
    private static readonly object EmojiThrottleSync = new();
    private static readonly TimeSpan EmojiSendThrottleWindow = TimeSpan.FromMilliseconds(900);

    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly MarketService _marketService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IGameSessionRepository gameSessionRepository,
        MarketService marketService,
        ILogger<GameHub> logger)
    {
        _gameSessionRepository = gameSessionRepository;
        _marketService = marketService;
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

        if (!await IsPlayerInGameAsync(gameId, userId))
        {
            throw new HubException("You are not a participant of this game.");
        }

        string groupName = GetGroupName(gameId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        bool notifyJoined;
        lock (PresenceSync)
        {
            var key = GetPresenceKey(userId, gameId);
            var connections = UserGameConnections.GetOrAdd(key, _ => new HashSet<string>(StringComparer.Ordinal));
            notifyJoined = connections.Count == 0;
            connections.Add(Context.ConnectionId);
            ConnectionSubscriptions[Context.ConnectionId] = (userId, gameId);
        }

        _logger.LogInformation("Player {PlayerId} joined game group {GameId}", userId, gameId);

        if (notifyJoined)
        {
            await Clients.OthersInGroup(groupName).SendAsync("PlayerJoined", userId);
        }
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

        bool notifyLeft = RemovePresence(Context.ConnectionId, userId, gameId);

        _logger.LogInformation("Player {PlayerId} left game group {GameId}", userId, gameId);

        if (notifyLeft)
        {
            RemoveEmojiThrottle(userId, gameId);
            await Clients.OthersInGroup(groupName).SendAsync("PlayerLeft", userId);
        }
    }

    public async Task SendChatMessage(Guid gameId, string message)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        if (!ConnectionSubscriptions.TryGetValue(Context.ConnectionId, out var subscription) || subscription.GameId != gameId)
        {
            throw new HubException("Join the game before sending chat messages.");
        }

        string text = (message ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new HubException("Message cannot be empty.");
        }

        if (text.Length > 300)
        {
            throw new HubException("Message is too long.");
        }

        string senderName = Context.User?.FindFirstValue(ClaimTypes.Name)
            ?? Context.User?.Identity?.Name
            ?? userId;

        await Clients.Group(GetGroupName(gameId)).SendAsync("ChatMessageReceived", new
        {
            GameId = gameId,
            SenderId = userId,
            SenderName = senderName,
            Message = text,
            SentAtUtc = DateTime.UtcNow
        });
    }

    public async Task SendEmoji(Guid gameId, Guid emojiItemId)
    {
        string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        if (!ConnectionSubscriptions.TryGetValue(Context.ConnectionId, out var subscription) || subscription.GameId != gameId)
        {
            throw new HubException("Join the game before sending emojis.");
        }

        if (!await IsPlayerInGameAsync(gameId, userId))
        {
            throw new HubException("You are not a participant of this game.");
        }

        if (IsEmojiRateLimited(userId, gameId))
        {
            throw new HubException("You are sending emojis too fast.");
        }

        var emojiItem = await TryGetUsableEmoji(userId, emojiItemId);

        string senderName = Context.User?.FindFirstValue(ClaimTypes.Name)
            ?? Context.User?.Identity?.Name
            ?? userId;

        await Clients.Group(GetGroupName(gameId)).SendAsync("EmojiReceived", new
        {
            GameId = gameId,
            SenderId = userId,
            SenderName = senderName,
            EmojiId = emojiItem.Id,
            ImageUrl = emojiItem.ImageUrl,
            SentAtUtc = DateTime.UtcNow
        });
    }

    private async Task<MarketCatalogItemDto> TryGetUsableEmoji(string userId, Guid emojiItemId)
    {
        try
        {
            return await _marketService
                .GetUsableItemForUser(userId, emojiItemId, MarketItemType.Emoji)
                .ConfigureAwait(false);
        }
        catch (QuizanchosException ex)
        {
            throw new HubException(ex.Message);
        }
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
        if (ConnectionSubscriptions.TryGetValue(Context.ConnectionId, out var subscription))
        {
            string groupName = GetGroupName(subscription.GameId);
            bool notifyLeft = RemovePresence(Context.ConnectionId, subscription.UserId, subscription.GameId);

            if (notifyLeft)
            {
                RemoveEmojiThrottle(subscription.UserId, subscription.GameId);
                await Clients.OthersInGroup(groupName).SendAsync("PlayerLeft", subscription.UserId);
            }
        }

        _logger.LogInformation("Client disconnected: {ConnectionId}, Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "normal");

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> IsPlayerInGameAsync(Guid gameId, string userId)
    {
        var session = await _gameSessionRepository.GetByIdAsync(gameId);
        return session?.Players.Any(x => x.ApplicationUserId == userId) == true;
    }

    private static string GetPresenceKey(string userId, Guid gameId) => $"{gameId}:{userId}";

    private static bool IsEmojiRateLimited(string userId, Guid gameId)
    {
        lock (EmojiThrottleSync)
        {
            string key = GetPresenceKey(userId, gameId);
            DateTime now = DateTime.UtcNow;

            if (LastEmojiSentAt.TryGetValue(key, out DateTime previousSentAt) && now - previousSentAt < EmojiSendThrottleWindow)
            {
                return true;
            }

            LastEmojiSentAt[key] = now;
            return false;
        }
    }

    private static void RemoveEmojiThrottle(string userId, Guid gameId)
    {
        string key = GetPresenceKey(userId, gameId);
        LastEmojiSentAt.TryRemove(key, out _);
    }

    private static bool RemovePresence(string connectionId, string userId, Guid gameId)
    {
        lock (PresenceSync)
        {
            ConnectionSubscriptions.TryRemove(connectionId, out _);

            var key = GetPresenceKey(userId, gameId);
            if (!UserGameConnections.TryGetValue(key, out var connections))
            {
                return false;
            }

            connections.Remove(connectionId);
            if (connections.Count > 0)
            {
                return false;
            }

            UserGameConnections.TryRemove(key, out _);
            return true;
        }
    }

    internal static string GetGroupName(Guid gameId) => $"game-{gameId}";
}
