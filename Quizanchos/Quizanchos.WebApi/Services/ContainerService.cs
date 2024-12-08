namespace Quizanchos.WebApi.Services;

public class ContainerService
{
    public Dictionary<Guid, EmailConfirmationUserRegistrationService.UserData> PendingUsersDictionary { get; } = new();
}
