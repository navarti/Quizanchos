using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services.GameLogic;

/// <summary>
/// Factory for creating and loading game engines.
/// Uses the minigame registry plugin system to support multiple minigames
/// without hardcoding dependencies on specific implementations.
/// </summary>
public class GameLogicFactory : IGameLogicFactory
{
    private readonly ILogger<GameLogicFactory> _logger;
    private readonly IMinigameRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    public GameLogicFactory(
        ILogger<GameLogicFactory> logger,
        IMinigameRegistry registry,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    public async Task<IGameEngine> CreateGameEngine(int type, Guid gameId, ImmutableArray<string> playerIds, Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Creating game engine for type: {Type}, GameId: {GameId}, Players: {PlayerCount}",
            type, gameId, playerIds.Length);

        var descriptor = _registry.GetDescriptor(type);
        if (descriptor == null)
        {
            throw new ArgumentException($"Unknown minigame type: {type}. No descriptor registered.");
        }

        return await descriptor.CreateGameEngineAsync(gameId, playerIds, parameters, _serviceProvider);
    }

    public async Task<IGameEngine?> LoadGameEngine(int type, Guid gameId)
    {
        _logger.LogInformation("Loading game engine for type: {Type}, GameId: {GameId}", type, gameId);

        var descriptor = _registry.GetDescriptor(type);
        if (descriptor == null)
        {
            throw new ArgumentException($"Unknown minigame type: {type}. No descriptor registered.");
        }

        return await descriptor.LoadGameEngineAsync(gameId, _serviceProvider);
    }

    public async Task SaveGameState(int type, Guid gameId, IGameState state)
    {
        _logger.LogInformation("Saving game state for type: {Type}, GameId: {GameId}", type, gameId);

        var descriptor = _registry.GetDescriptor(type);
        if (descriptor == null)
        {
            throw new ArgumentException($"Unknown minigame type: {type}. No descriptor registered.");
        }

        await descriptor.SaveGameStateAsync(gameId, state, _serviceProvider);
    }
}
