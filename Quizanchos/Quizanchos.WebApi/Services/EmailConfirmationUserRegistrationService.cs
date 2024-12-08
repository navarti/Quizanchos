using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Services.Other;

namespace Quizanchos.WebApi.Services;

public class EmailConfirmationUserRegistrationService : IUserRegistrationService
{
    public class UserData
    {
        public ApplicationUser User { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public DateTime RequestedTime { get; set; }
    }

    private readonly EmailSenderService _emailSenderService;
    private readonly DefaultUserRegistrationService _defaultUserRegistrationService;
    private readonly ContainerService _containerService;

    public EmailConfirmationUserRegistrationService(DefaultUserRegistrationService defaultUserRegistrationService, EmailSenderService emailSenderService, ContainerService containerService)
    {
        _defaultUserRegistrationService = defaultUserRegistrationService;
        _emailSenderService = emailSenderService;
        _containerService = containerService;
    }

    public async Task<RegisterUserResult> RegisterUser(ApplicationUser user, string password, string roleName)
    {
        Guid id = Guid.NewGuid();
        UserData userData = new()
        {
            User = user,
            Password = password,
            RoleName = roleName,
            RequestedTime = DateTime.Now
        };

        _containerService.PendingUsersDictionary.Add(id, userData);

        string title = "Confirm your email";
        string content = $"<a href='https://localhost:7020/EmailConfirmation/ConfirmEmail/{id}'>Click here to confirm your email</a>";

        await _emailSenderService.SendHtmlEmailAsync(user.Email, title, content);

        return RegisterUserResult.PendingConfirmation;
    }

    public async Task ConfirmEmail(Guid guid)
    {
        if (!_containerService.PendingUsersDictionary.TryGetValue(guid, out UserData userData))
        {
            throw HandledExceptionFactory.Create("The application with this id does not exist");
        }

        _containerService.PendingUsersDictionary.Remove(guid);

        if (DateTime.Now - userData.RequestedTime > TimeSpan.FromHours(1))
        {
            throw HandledExceptionFactory.Create("The confirmation link has expired");
        }

        await _defaultUserRegistrationService.RegisterUser(userData.User, userData.Password, userData.RoleName);
    }
}
