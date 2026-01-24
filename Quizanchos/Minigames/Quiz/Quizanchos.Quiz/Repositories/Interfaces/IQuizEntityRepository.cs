using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Repositories.Interfaces;

public interface IQuizEntityRepository : IEntityRepository<Guid, QuizEntity>
{
    Task<QuizEntity?> FindByName(string name);
}
