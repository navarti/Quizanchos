using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.WebApi.Services.GameLogic;

public sealed class GameStatePersistence : IGameStatePersistence
{
    private readonly IGameSessionRepository _sessions;
    private readonly IGameSessionStateRepository _states;

    public GameStatePersistence(
        IGameSessionRepository sessions,
        IGameSessionStateRepository states)
    {
        _sessions = sessions;
        _states = states;
    }

    public async Task CreateAsync(Guid gameId, int minigameTypeId, IReadOnlyList<string> playerIds, string stateJson)
    {
        var now = DateTime.UtcNow;
        var session = new GameSession
        {
            Id = gameId,
            MinigameType = minigameTypeId,
            IsActive = true,
            IsFinished = false,
            CreatedAt = now,
        };
        foreach (var playerId in playerIds)
        {
            session.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId,
                JoinedAt = now,
            });
        }
        await _sessions.CreateAsync(session);

        var sessionState = new GameSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameId,
            MinigameType = minigameTypeId,
            StateJson = stateJson,
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _states.CreateAsync(sessionState);
    }

    public async Task<LoadedState?> LoadAsync(Guid gameId)
    {
        var sessionState = await _states.GetByGameSessionIdAsync(gameId);
        if (sessionState is null)
        {
            return null;
        }

        var session = sessionState.GameSession;
        var playerIds = session.Players
            .Select(p => p.ApplicationUserId)
            .ToList();

        return new LoadedState(
            sessionState.StateJson,
            playerIds,
            session.IsFinished,
            session.WinnerId);
    }

    public async Task UpdateAsync(Guid gameId, string stateJson)
    {
        var sessionState = await _states.GetByGameSessionIdAsync(gameId);
        if (sessionState is null)
        {
            throw new InvalidOperationException($"GameSessionState not found for GameSessionId: {gameId}");
        }

        sessionState.StateJson = stateJson;
        sessionState.UpdatedAt = DateTime.UtcNow;
        await _states.UpdateAsync(sessionState);
    }
}
