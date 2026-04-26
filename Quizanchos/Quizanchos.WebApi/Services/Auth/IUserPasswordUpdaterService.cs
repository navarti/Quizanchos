namespace Quizanchos.WebApi.Services.Auth;

public interface IUserPasswordUpdaterService
{
    Task RequestPasswordResetAsync(string email);
    Task ConfirmPasswordResetAsync(string email, string code, string newPassword);
}
