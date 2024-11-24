using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizCardService
{
    public Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex);
    public Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession);
    public Task<QuizCardAbstract> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked);
}
