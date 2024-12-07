using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services;

public class EmailConfirmationPasswordUpdaterService : IUserPasswordUpdaterService
{
    public class UserData
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    private readonly EmailSenderService _emailSenderService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ContainerService _containerService;

    public EmailConfirmationPasswordUpdaterService(EmailSenderService emailSenderService, UserManager<ApplicationUser> userManager, ContainerService containerService)
    {
        _emailSenderService = emailSenderService;
        _containerService = containerService;
        _userManager = userManager;
    }

    public async Task<RegisterUserResult> UpdatePasswordAsync(string email, string newPassword)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        _ = user ?? throw HandledExceptionFactory.Create("User not found");
        
        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        UserData userData = new()
        {
            UserId = user.Id,
            Password = newPassword,
        };

        _containerService.PendingPasswordDictionary.Add(code, userData);

        string title = "Confirm your email";
        string content = $"Your code to confirm email: {code}";

        await _emailSenderService.SendEmailAsync(user.Email, title, content);

        return RegisterUserResult.PendingConfirmation;
    }

    public async Task ConfirmEmail(string code)
    {
        if (!_containerService.PendingPasswordDictionary.TryGetValue(code, out UserData userData))
        {
            throw HandledExceptionFactory.Create("The application with this code does not exist");
        }

        _containerService.PendingPasswordDictionary.Remove(code);

        ApplicationUser? user = await _userManager.FindByIdAsync(userData.UserId);
        _ = user ?? throw HandledExceptionFactory.Create("User not found");
        await _userManager.ResetPasswordAsync(user, code, userData.Password);
    }
}
