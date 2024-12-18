using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureIntRepository : EntityRepositoryBase<Guid, FeatureInt>, IFeatureIntRepository
{
    public FeatureIntRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FeatureInt> GetByIdIncluding(Guid id)
    {
        return await _dbSet
            .Include(s => s.QuizEntity)
            .Include(s => s.QuizCategory)
            .FirstOrDefaultAsync(s => s.Id == id) ?? throw HandledExceptionFactory.CreateIdNotFoundException(id);
    }

    public async Task<List<FeatureInt>> GetByCategoryId(Guid categoryId)
    {
        return await _dbSet
            .Include(s => s.QuizEntity)
            .Include(s => s.QuizCategory)
            .Where(s => s.QuizCategory.Id == categoryId)
            .ToListAsync();
    }

    public Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId)
    {
        return _dbSet.FirstOrDefaultAsync(feature => feature.QuizCategory.Id == categoryId && feature.QuizCategory.Id == entityId);
    }

    public async Task<FeatureInt?> FindRandomByCategory(Guid categoryId, double coefficient, int? lastValue = null)
    {
        var query = _dbSet
            .Include(feature => feature.QuizEntity)
            .Where(feature => feature.QuizCategory.Id == categoryId);
        
        int totalFeatures = await query.CountAsync();

        if (totalFeatures == 0)
        {
            return null;
        }

        int subsetSize = (int)Math.Ceiling(totalFeatures * coefficient);

        List<FeatureInt> features = await query.OrderBy(feature => feature.Value).ToListAsync();

        List<FeatureInt> subset;
        if (lastValue.HasValue)
        {
            subset = features
                .OrderBy(feature => Math.Abs(feature.Value.Value - lastValue.Value))
                .Take(subsetSize)
                .ToList();
        }
        else
        {
            subset = features.Take(subsetSize).ToList();
        }

        return subset.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
    }
}
