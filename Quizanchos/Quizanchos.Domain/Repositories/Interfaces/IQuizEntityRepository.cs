using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizEntityRepository : IEntityRepository<Guid, QuizEntity>
{
}
