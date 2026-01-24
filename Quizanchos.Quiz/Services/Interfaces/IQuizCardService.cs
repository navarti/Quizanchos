using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using Quizanchos.Quiz.Dto.Abstractions;

namespace Quizanchos.Quiz.Services.Interfaces;

public interface IQuizCardService
{
    Task<QuizCardAbstract?> FindCardForSession(Guid gameSessionid, int cardIndex);
    Task<QuizCardAbstract> CreateCardForSession(SingleGameSession gameSession);
    Task<(QuizCardAbstract QuizCard, bool IsCorrect)> PickAnswerForSession(Guid gameSessionid, int cardIndex, int optionPicked);
    QuizCardDtoAbstract MapQuizCardDto(QuizCardAbstract quizCard);
    QuizCardDtoAbstract MapQuizCardDtoWithAnswer(QuizCardAbstract quizCard);
}
