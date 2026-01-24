using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Repositories.Interfaces;

public interface IQuizCardFloatRepository : IEntityRepository<Guid, QuizCardFloat>
{
    Task<QuizCardFloat?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex);
    Task<QuizCardFloat> PickAnswerForSession(Guid gameSessionid, int cardIndex, int answer);
}
