using Quizanchos.Common.Util;

namespace Quizanchos.WebApi.Services.Auth;

// Used when EmailConfirmation:ShouldUse = "0". Password reset by email is not
// available because no email channel is configured, so both endpoints refuse
// rather than silently succeeding (the previous implementation reset any
// account by email alone, which was a full-takeover bug).
public class DefaultPasswordUpdaterService : IUserPasswordUpdaterService
{
    public Task RequestPasswordResetAsync(string email)
    {
        throw HandledExceptionFactory.Create(
            "Password reset is unavailable: email confirmation is disabled on this server.");
    }

    public Task ConfirmPasswordResetAsync(string email, string code, string newPassword)
    {
        throw HandledExceptionFactory.Create(
            "Password reset is unavailable: email confirmation is disabled on this server.");
    }
}
