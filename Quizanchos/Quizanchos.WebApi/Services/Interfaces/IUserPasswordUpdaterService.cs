using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IUserPasswordUpdaterService
{
    Task<RegisterUserResult> UpdatePasswordAsync(string email, string newPassword);
}
