namespace Quizanchos.ViewModels.Admin;

public enum AdminAlertSeverity { Info, Success, Warning, Error }

public class AdminAlertViewModel
{
    public required string Message { get; init; }
    public AdminAlertSeverity Severity { get; init; } = AdminAlertSeverity.Info;
    public string? Title { get; init; }
}
