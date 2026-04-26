namespace Quizanchos.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public string PageTitle { get; init; } = "Admin Dashboard";
    public string PageSubtitle { get; init; } = "Manage users, content, top-ups and platform settings.";

    public List<AdminStatCardViewModel> Stats { get; init; } = new();
    public List<AdminToolCardViewModel> Tools { get; init; } = new();

    /// <summary>
    /// Set when an upstream data source failed; views render a non-blocking
    /// banner so the rest of the dashboard remains usable.
    /// </summary>
    public string? LoadError { get; init; }
}
