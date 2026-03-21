namespace Quizanchos.Core;

/// <summary>
/// Registry for managing minigame frontend descriptors.
/// Allows dynamic discovery and management of minigames in UI.
/// </summary>
public interface IMinigameFrontendRegistry
{
    void Register(IMinigameFrontendDescriptor descriptor);
    IMinigameFrontendDescriptor? GetDescriptor(string gameKey);
    IMinigameFrontendDescriptor? GetDescriptor(int minigameTypeId);
    IReadOnlyDictionary<string, IMinigameFrontendDescriptor> GetAllDescriptors();
    bool IsRegistered(string gameKey);
    bool IsRegistered(int minigameTypeId);
}
