namespace Quizanchos.WebApi.Services;

public class ContainerService
{
    public Random Random { get; } = new();
    public Dictionary<string, EmailConfirmationUserRegistrationService.UserData> PendingUsersDictionary { get; } = new();
    public Dictionary<string, EmailConfirmationPasswordUpdaterService.UserData> PendingPasswordDictionary { get; } = new();
}
