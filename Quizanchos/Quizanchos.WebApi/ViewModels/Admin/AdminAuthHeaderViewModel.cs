namespace Quizanchos.ViewModels.Admin;

/// <summary>
/// Header used inside the admin auth shell (sign-in / sign-up).
/// </summary>
public class AdminAuthHeaderViewModel
{
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
}
