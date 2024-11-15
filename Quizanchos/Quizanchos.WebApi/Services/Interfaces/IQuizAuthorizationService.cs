using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizAuthorizationService
{
    Task Login(LoginModelDto loginModelDto);

    Task RegisterUser(RegisterModelDto registerModelDto);
}
