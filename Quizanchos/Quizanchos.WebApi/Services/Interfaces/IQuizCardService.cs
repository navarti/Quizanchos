using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.WebApi.Dto.Abstractions;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizCardService
{
    Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex);
    Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession);
    Task<(QuizCardAbstract QuizCard, bool IsCorrect)> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked);
    QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard);
    QuizCardDtoAbstract MapQuizCardDtoWithAnswer(QuizCardAbstract quizCard);
}
