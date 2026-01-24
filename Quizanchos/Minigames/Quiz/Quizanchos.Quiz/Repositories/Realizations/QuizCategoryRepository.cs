using Microsoft.EntityFrameworkCore;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Repositories.Interfaces;

namespace Quizanchos.Quiz.Repositories.Realizations;

public class QuizCategoryRepository : EntityRepositoryBase<Guid, QuizCategory>, IQuizCategoryRepository
{
    public QuizCategoryRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<QuizCategory?> FindByName(string name)
    {
        return _dbSet.FirstOrDefaultAsync(quizCategory => quizCategory.Name == name);
    }
}
