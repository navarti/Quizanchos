namespace Quizanchos.ViewModels.Admin;

/// <summary>
/// A management tool tile shown on the admin dashboard. Either opens a modal
/// (<see cref="OpenModalId"/>) or links to a route (<see cref="ActionUrl"/>).
/// </summary>
public class AdminToolCardViewModel
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string IconKey { get; init; }
    public required string ActionLabel { get; init; }
    public string? ActionUrl { get; init; }
    public string? OpenModalId { get; init; }
    public bool IsEnabled { get; init; } = true;
    public string? DisabledReason { get; init; }
}
