namespace Quizanchos.ViewModels;

public class MinigameCardViewModel
{
    public int MinigameTypeId { get; set; }
    public required string GameKey { get; set; }
    public required string DisplayName { get; set; }
    public bool IsPremium { get; set; }
    public bool CanAccess { get; set; }
    public required string Description { get; set; }
    public required string CardStyle { get; set; }
    public required string LobbyUrl { get; set; }
    public string? GameUrlTemplate { get; set; }
    public required string ActionText { get; set; }
    public int Order { get; set; }
}
