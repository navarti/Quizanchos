using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizCardIntRepository : IEntityRepository<Guid, QuizCardInt>
{
    Task<QuizCardInt?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex);
}
