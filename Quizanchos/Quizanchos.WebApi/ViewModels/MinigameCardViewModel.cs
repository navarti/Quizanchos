namespace Quizanchos.ViewModels;

public class MinigameCardViewModel
{
    public required string GameKey { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public required string CardStyle { get; set; }
    public required string LobbyUrl { get; set; }
    public required string ActionText { get; set; }
    public int Order { get; set; }
}
