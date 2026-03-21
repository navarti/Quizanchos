namespace Quizanchos.Core;

/// <summary>
/// Registry for managing minigame descriptors.
/// Allows dynamic discovery and management of minigames.
/// </summary>
public interface IMinigameRegistry
{
    /// <summary>
    /// Register a minigame descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor to register</param>
    /// <exception cref="ArgumentException">If the descriptor has an empty GameKey</exception>
    void Register(IMinigameDescriptor descriptor);

    /// <summary>
    /// Get a descriptor by game key.
    /// </summary>
    /// <param name="gameKey">The unique key for the minigame</param>
    /// <returns>The descriptor, or null if not found</returns>
    IMinigameDescriptor? GetDescriptor(string gameKey);

    /// <summary>
    /// Get all registered descriptors.
    /// </summary>
    /// <returns>Read-only dictionary of all registered descriptors keyed by GameKey</returns>
    IReadOnlyDictionary<string, IMinigameDescriptor> GetAllDescriptors();

    /// <summary>
    /// Check if a descriptor is registered for the given key.
    /// </summary>
    bool IsRegistered(string gameKey);
}
