using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Quiz.Interfaces;

public interface IQuizEntityRepository : IEntityRepository<Guid, QuizEntity>
{
    Task<QuizEntity?> FindByName(string name);
}
