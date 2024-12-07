using Quizanchos.Common.Util;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Services.Other;

namespace Quizanchos.WebApi.Services;

public class DummyPasswordUpdaterService : IUserPasswordUpdaterService
{
    public Task<RegisterUserResult> UpdatePasswordAsync(string email, string newPassword)
    {
        string message = "This feature is currently not available due to expensive cost of Api for email confirmation. " +
            "If you forget your password and need it so badly, you are welcome to gift it us (we use MailGun Api)";
        
        throw HandledExceptionFactory.Create(message);
    }
}
