using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace Quizanchos.Core;

/// <summary>
/// Descriptor for a minigame plugin. Allows self-registration of minigames
/// without modifying the WebApi code.
/// </summary>
public interface IMinigameDescriptor
{
    /// <summary>
    /// Unique identifier for the minigame (e.g., "Quiz", "Game2048", "QuizMultiplayer").
    /// This should match the MinigameType enum value name.
    /// </summary>
    string GameKey { get; }

    /// <summary>
    /// Human-readable display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Register this minigame's services with the DI container.
    /// This method is called during application startup.
    /// </summary>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Create a game engine for this minigame type.
    /// </summary>
    /// <param name="gameId">The unique identifier for the game instance</param>
    /// <param name="playerIds">Array of player IDs participating in the game</param>
    /// <param name="parameters">Game-specific parameters (e.g., difficulty, board size, etc.)</param>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <returns>A new game engine instance</returns>
    Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider);

    /// <summary>
    /// Load a previously saved game engine from storage.
    /// </summary>
    /// <param name="gameId">The unique identifier for the game instance</param>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <returns>The loaded game engine, or null if not found</returns>
    Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider);

    /// <summary>
    /// Save game state for persistence.
    /// </summary>
    /// <param name="gameId">The unique identifier for the game instance</param>
    /// <param name="state">The game state to persist</param>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider);
}
