namespace Quizanchos.ViewModels;

public class MinigameHostViewModel
{
    public required string GameKey { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string LobbyUrl { get; init; }
    public required string GameUrlTemplate { get; init; }
    public required int MinigameTypeId { get; init; }
    public required string ViewMode { get; init; }
    public Guid? GameId { get; init; }
    public IReadOnlyList<string> Styles { get; init; } = [];
    public IReadOnlyList<string> Scripts { get; init; } = [];
}
