using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizCardFloatRepository : IEntityRepository<Guid, QuizCardFloat>
{
    Task<QuizCardFloat?> FindCardForSessionIncluding(Guid gameSessionid, int cardIndex);
}
