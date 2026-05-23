using Microsoft.AspNetCore.SignalR;
using Quizanchos.Core;
using Quizanchos.WebApi.Hubs;

namespace Quizanchos.WebApi.Services.GameLogic;

/// <summary>
/// SignalR-based implementation of <see cref="IGameNotifier"/>.
/// Pushes game events to all clients in the SignalR group for a given game.
/// </summary>
public class SignalRGameNotifier : IGameNotifier
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<SignalRGameNotifier> _logger;

    public SignalRGameNotifier(IHubContext<GameHub> hubContext, ILogger<SignalRGameNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyStateChanged(Guid gameId, IGameState state)
    {
        string groupName = GameHub.GetGroupName(gameId);
        _logger.LogInformation("Broadcasting state change for game {GameId}", gameId);

        await _hubContext.Clients.Group(groupName).SendAsync("GameStateChanged", new
        {
            GameId = gameId,
            State = (object)state,
            IsFinished = state.IsFinished,
            Winner = state.Winner
        });
    }

    public async Task NotifyMoveMade(Guid gameId, string playerId, IGameState state)
    {
        string groupName = GameHub.GetGroupName(gameId);
        _logger.LogInformation("Broadcasting move by {PlayerId} for game {GameId}", playerId, gameId);

        await _hubContext.Clients.Group(groupName).SendAsync("MoveMade", new
        {
            GameId = gameId,
            PlayerId = playerId,
            State = (object)state,
            IsFinished = state.IsFinished,
            Winner = state.Winner
        });
    }

    public async Task NotifyGameFinished(Guid gameId, IGameState state, string? winner)
    {
        string groupName = GameHub.GetGroupName(gameId);
        _logger.LogInformation("Broadcasting game finished for game {GameId}, Winner: {Winner}", gameId, winner);

        await _hubContext.Clients.Group(groupName).SendAsync("GameFinished", new
        {
            GameId = gameId,
            State = (object)state,
            Winner = winner
        });
    }

    public async Task NotifyPlayerJoined(Guid gameId, string playerId)
    {
        string groupName = GameHub.GetGroupName(gameId);
        _logger.LogInformation("Broadcasting player {PlayerId} joined game {GameId}", playerId, gameId);

        await _hubContext.Clients.Group(groupName).SendAsync("PlayerJoined", playerId);
    }

    public async Task NotifyPlayerLeft(Guid gameId, string playerId)
    {
        string groupName = GameHub.GetGroupName(gameId);
        _logger.LogInformation("Broadcasting player {PlayerId} left game {GameId}", playerId, gameId);

        await _hubContext.Clients.Group(groupName).SendAsync("PlayerLeft", playerId);
    }
}
