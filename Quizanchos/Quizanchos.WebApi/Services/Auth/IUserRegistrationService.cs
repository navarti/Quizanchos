using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Util;

namespace Quizanchos.WebApi.Services.Auth;

public interface IUserRegistrationService
{
    Task<RegisterUserResult> RegisterUser(ApplicationUser applicationUser, string password, string roleName);
}
