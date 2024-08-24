using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IQuizCategoryRepository : IEntityRepository<Guid, QuizCategory>
{
}
