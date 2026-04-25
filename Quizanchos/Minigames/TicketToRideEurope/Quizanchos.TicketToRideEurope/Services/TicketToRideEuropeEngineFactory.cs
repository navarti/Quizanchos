using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.TicketToRideEurope.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.TicketToRideEurope.Services;

public class TicketToRideEuropeEngineFactory
{
    private readonly ILogger<TicketToRideEuropeEngineFactory> _logger;
    private readonly TicketToRideEuropeStateService _stateService;
    private readonly IGameSessionRepository _gameSessionRepository;

    public TicketToRideEuropeEngineFactory(
        ILogger<TicketToRideEuropeEngineFactory> logger,
        TicketToRideEuropeStateService stateService,
        IGameSessionRepository gameSessionRepository)
    {
        _logger = logger;
        _stateService = stateService;
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>> CreateEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds)
    {
        _logger.LogInformation(
            "Creating TicketToRideEurope engine: GameId={GameId}, Players={PlayerCount}",
            gameId, playerIds.Length);

        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = TicketToRideEuropeState.MinigameTypeId,
            IsActive = true,
            IsFinished = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (string playerId in playerIds)
        {
            gameSession.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _gameSessionRepository.CreateAsync(gameSession);

        TicketToRideEuropeLogic logic = new TicketToRideEuropeLogic();
        GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove> engine =
            new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, gameId, playerIds);

        await _stateService.CreateInitialStateAsync(gameId, engine.State);

        _logger.LogInformation("TicketToRideEurope engine created for GameId={GameId}", gameId);
        return engine;
    }

    public async Task<GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading TicketToRideEurope engine for GameId={GameId}", gameId);

        TicketToRideEuropeState? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("TicketToRideEurope state not found for GameId={GameId}", gameId);
            return null;
        }

        TicketToRideEuropeLogic logic = new TicketToRideEuropeLogic();
        GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove> engine =
            new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, state);

        return engine;
    }

    public async Task SaveStateAsync(Guid gameId, TicketToRideEuropeState state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
