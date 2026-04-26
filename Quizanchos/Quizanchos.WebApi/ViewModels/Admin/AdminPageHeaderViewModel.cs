namespace Quizanchos.ViewModels.Admin;

/// <summary>
/// Shared header for any admin page. Use the partial `_AdminPageHeader`.
/// </summary>
public class AdminPageHeaderViewModel
{
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public string? BackUrl { get; init; }
    public string? BackLabel { get; init; } = "Back to Admin";
}
