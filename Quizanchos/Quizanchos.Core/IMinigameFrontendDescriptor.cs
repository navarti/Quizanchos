namespace Quizanchos.Core;

/// <summary>
/// Descriptor for frontend representation of a minigame plugin.
/// Allows UI to render minigame cards and links without hardcoding minigame logic.
/// </summary>
public interface IMinigameFrontendDescriptor
{
    int MinigameTypeId { get; }
    string GameKey { get; }
    string DisplayName { get; }
    string Description { get; }
    string CardStyle { get; }
    string LobbyUrl { get; }
    string GameUrlTemplate { get; }
    string ActionText { get; }
    int Order { get; }
}
