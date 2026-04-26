namespace Quizanchos.ViewModels.Admin;

/// <summary>
/// A single KPI tile shown on the admin dashboard.
/// `Value` is rendered exactly as supplied — controllers should pre-format
/// (e.g. "1,247", "—" when unavailable).
/// </summary>
public class AdminStatCardViewModel
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Value { get; init; }
    public string? Caption { get; init; }
    public string? IconKey { get; init; }
    public string? Accent { get; init; } = "primary";
    public bool IsAvailable { get; init; } = true;
    public string? UnavailableReason { get; init; }
}
