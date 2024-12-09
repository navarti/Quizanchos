using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.HelperServices;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

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

    private const int CodeLength = 6;

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
        string code = StringGenerator.GenerateRandomString(_containerService.Random, CodeLength);
        UserData userData = new()
        {
            User = user,
            Password = password,
            RoleName = roleName,
            RequestedTime = DateTime.Now
        };

        _containerService.PendingUsersDictionary.Add(code, userData);

        string title = "Confirm your email";
        string content = $"Your code to confirm email: {code}";

        await _emailSenderService.SendEmailAsync(user.Email, title, content);

        return RegisterUserResult.PendingConfirmation;
    }

    public async Task ConfirmEmail([FromBody] string code)
    {
        if (!_containerService.PendingUsersDictionary.TryGetValue(code, out UserData userData))
        {
            throw HandledExceptionFactory.Create("The application with this code does not exist");
        }

        _containerService.PendingUsersDictionary.Remove(code);

        if (DateTime.Now - userData.RequestedTime > TimeSpan.FromHours(1))
        {
            throw HandledExceptionFactory.Create("The confirmation link has expired");
        }

        await _defaultUserRegistrationService.RegisterUser(userData.User, userData.Password, userData.RoleName);
    }
}
