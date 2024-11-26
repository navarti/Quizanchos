using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureIntRepository : EntityRepositoryBase<Guid, FeatureInt>, IFeatureIntRepository
{
    public FeatureIntRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId)
    {
        return _dbSet.FirstOrDefaultAsync(feature => feature.QuizCategory.Id == categoryId && feature.QuizCategory.Id == entityId);
    }

    public Task<FeatureInt?> FindRandomByCategory(Guid categoryId)
    {
        return _dbSet
            .Include(feature => feature.QuizEntity)
            .Where(feature => feature.QuizCategory.Id == categoryId)
            .OrderBy(feature => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }
}
