using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Auth;

public interface IUserPasswordUpdaterService
{
    Task<RegisterUserResult> UpdatePasswordAsync(string email, string newPassword);
}
