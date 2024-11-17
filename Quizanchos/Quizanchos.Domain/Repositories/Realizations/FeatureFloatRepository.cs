using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureFloatRepository : EntityRepositoryBase<Guid, FeatureFloat>, IFeatureFloatRepository
{
    public FeatureFloatRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId)
    {
        return _dbSet.FirstOrDefaultAsync(feature => feature.QuizCategory.Id == categoryId && feature.QuizEntity.Id == entityId);
    }
}
