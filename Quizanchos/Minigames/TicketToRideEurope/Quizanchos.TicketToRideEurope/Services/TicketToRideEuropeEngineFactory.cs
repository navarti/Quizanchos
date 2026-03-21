using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.TicketToRideEurope.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.TicketToRideEurope.Services;

public class TicketToRideEuropeEngineFactory
{
    private const int TicketToRideMinigameTypeId = 4;

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
        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = TicketToRideMinigameTypeId,
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

        var logic = new TicketToRideEuropeLogic();
        var engine = new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, gameId, playerIds);

        await _stateService.CreateInitialStateAsync(gameSession, engine.State);

        _logger.LogInformation("Ticket to Ride: Europe game created: {GameId}", gameId);
        return engine;
    }

    public async Task<GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>?> LoadEngineAsync(Guid gameId)
    {
        TicketToRideEuropeState? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("Ticket to Ride: Europe state not found for GameId={GameId}", gameId);
            return null;
        }

        var logic = new TicketToRideEuropeLogic();
        return new GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>(logic, state);
    }

    public async Task SaveStateAsync(Guid gameId, TicketToRideEuropeState state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
