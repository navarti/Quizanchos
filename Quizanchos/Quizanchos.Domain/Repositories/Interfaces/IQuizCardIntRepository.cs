using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizCardIntRepository : IEntityRepository<Guid, QuizCardInt>
{
    Task<QuizCardInt?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex);
    Task<QuizCardInt> PickAnswerForSession(Guid gameSessionid, int cardIndex, int answer);
}
