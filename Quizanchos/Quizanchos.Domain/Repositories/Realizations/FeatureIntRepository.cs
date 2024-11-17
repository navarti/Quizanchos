using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureIntRepository : EntityRepositoryBase<Guid, FeatureInt>, IFeatureIntRepository
{
    public FeatureIntRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId)
    {
        return _dbSet.FirstOrDefaultAsync(feature => feature.QuizCategoryId == categoryId && feature.QuizEntityId == entityId);
    }
}
