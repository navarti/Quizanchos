using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Domain.Repositories.Implementations;

namespace Quizanchos.Domain.Repositories.Quiz.Implementations;

public class QuizEntityRepository : EntityRepositoryBase<Guid, QuizEntity>, IQuizEntityRepository
{
    public QuizEntityRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public Task<QuizEntity?> FindByName(string name)
    {
        return _dbSet.FirstOrDefaultAsync(quizEntity => quizEntity.Name == name);
    }
}
