using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Services.Realizations;

public class AuthorizationService : IAuthorizationService
{
    public AuthorizationService()
    {
    }

    public Task<TokenDto> Login()
    {

        return new TokenDto();
    }
}
