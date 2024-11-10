using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizAuthorizationService
{
    Task<TokenDto> Login(LoginModelDto loginModelDto);

    Task<TokenDto> RegisterUser(RegisterModelDto registerModelDto);
}
