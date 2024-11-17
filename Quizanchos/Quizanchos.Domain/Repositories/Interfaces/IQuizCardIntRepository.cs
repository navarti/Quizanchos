using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizCardIntRepository : IEntityRepository<Guid, QuizCardInt>
{
    Task<QuizCardInt> GetCardForSession(Guid gameSessionid, int cardIndex);
}
