using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Domain.Repositories.Implementations;

namespace Quizanchos.Domain.Repositories.Quiz.Implementations;

public class QuizCategoryRepository : EntityRepositoryBase<Guid, QuizCategory>, IQuizCategoryRepository
{
    public QuizCategoryRepository(QuizanchosDbContext dbContext) : base(dbContext)
    {
    }

    public Task<QuizCategory?> FindByName(string name)
    {
        return _dbSet.FirstOrDefaultAsync(quizCategory => quizCategory.Name == name);
    }
}
