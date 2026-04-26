using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Auth;

public class EmailConfirmationPasswordUpdaterService : IUserPasswordUpdaterService
{
    public class UserData
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public DateTime RequestedTime { get; set; }
    }

    private const int CodeLength = 6;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromHours(1);

    private readonly EmailSenderService _emailSenderService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ContainerService _containerService;

    public EmailConfirmationPasswordUpdaterService(EmailSenderService emailSenderService, UserManager<ApplicationUser> userManager, ContainerService containerService)
    {
        _emailSenderService = emailSenderService;
        _containerService = containerService;
        _userManager = userManager;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        // Always succeed silently: do not leak whether an account exists for this email.
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return;
        }

        string code = StringGenerator.GenerateRandomString(_containerService.Random, CodeLength);
        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        UserData userData = new()
        {
            UserId = user.Id,
            Email = user.Email!,
            Token = token,
            RequestedTime = DateTime.UtcNow
        };

        lock (_containerService.PendingPasswordDictionary)
        {
            _containerService.PendingPasswordDictionary[code] = userData;
        }

        string title = "Reset your password";
        string content = $"Your password reset code: {code} (valid for 1 hour). If you did not request this, ignore this email.";

        await _emailSenderService.SendEmailAsync(user.Email!, title, content);
    }

    public async Task ConfirmPasswordResetAsync(string email, string code, string newPassword)
    {
        UserData? userData;
        lock (_containerService.PendingPasswordDictionary)
        {
            if (!_containerService.PendingPasswordDictionary.TryGetValue(code, out userData))
            {
                throw HandledExceptionFactory.Create("Invalid or expired reset code");
            }
            _containerService.PendingPasswordDictionary.Remove(code);
        }

        if (DateTime.UtcNow - userData.RequestedTime > CodeLifetime)
        {
            throw HandledExceptionFactory.Create("Invalid or expired reset code");
        }

        if (!string.Equals(userData.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            throw HandledExceptionFactory.Create("Invalid or expired reset code");
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(userData.UserId);
        _ = user ?? throw HandledExceptionFactory.Create("Invalid or expired reset code");

        IdentityResult result = await _userManager.ResetPasswordAsync(user, userData.Token, newPassword);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create(string.Concat(result.Errors.Select(e => e.Description)));
        }
    }
}
