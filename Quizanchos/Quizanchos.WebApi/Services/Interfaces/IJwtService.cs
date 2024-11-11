using Quizanchos.Domain.Entities;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateAcessTokenAsync(ApplicationUser user);
}