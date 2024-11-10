using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IAuthorizationService
{
    Task<TokenDto> Login();
}
