using Microsoft.EntityFrameworkCore;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Repositories.Interfaces;

namespace Quizanchos.Quiz.Repositories.Realizations;

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
