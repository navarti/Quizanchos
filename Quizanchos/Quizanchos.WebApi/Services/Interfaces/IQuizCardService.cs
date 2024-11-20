using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizCardService
{
    public Task<QuizCardAbstract> GetCardForSession(Guid gameSessionid, int cardIndex);
    public Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession);
}
