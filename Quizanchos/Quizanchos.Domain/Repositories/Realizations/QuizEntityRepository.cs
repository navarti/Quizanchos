using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class QuizEntityRepository : EntityRepositoryBase<Guid, QuizEntity>, IQuizEntityRepository
{
    public QuizEntityRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }
}
