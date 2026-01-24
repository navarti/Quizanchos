using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Repositories.Interfaces;

public interface IQuizCardIntRepository : IEntityRepository<Guid, QuizCardInt>
{
    Task<QuizCardInt?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex);
    Task<QuizCardInt> PickAnswerForSession(Guid gameSessionid, int cardIndex, int answer);
}
