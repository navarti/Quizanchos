using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Quiz.Interfaces;

public interface IQuizCategoryRepository : IEntityRepository<Guid, QuizCategory>
{
    public Task<QuizCategory?> FindByName(string name);
}
