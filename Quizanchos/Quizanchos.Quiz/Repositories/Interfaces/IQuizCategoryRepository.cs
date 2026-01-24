using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Repositories.Interfaces;

public interface IQuizCategoryRepository : IEntityRepository<Guid, QuizCategory>
{
    public Task<QuizCategory?> FindByName(string name);
}
