using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureFloatRepository : EntityRepositoryBase<Guid, FeatureFloat>, IFeatureFloatRepository
{
    public FeatureFloatRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FeatureFloat> GetByIdIncluding(Guid id)
    {
        return await _dbSet
            .Include(s => s.QuizEntity)
            .Include(s => s.QuizCategory)
            .FirstOrDefaultAsync(s => s.Id == id) ?? throw HandledExceptionFactory.CreateIdNotFoundException(id);
    }

    public async Task<List<FeatureFloat>> GetByCategoryId(Guid categoryId)
    {
        return await _dbSet
            .Include(s => s.QuizEntity)
            .Include(s => s.QuizCategory)
            .Where(s => s.QuizCategory.Id == categoryId)
            .ToListAsync();
    }

    public Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId)
    {
        return _dbSet.FirstOrDefaultAsync(feature => feature.QuizCategory.Id == categoryId && feature.QuizEntity.Id == entityId);
    }

    public async Task<FeatureFloat?> FindRandomByCategory(Guid categoryId, double coefficient, float? lastValue = null)
    {
        var query = _dbSet
            .Include(feature => feature.QuizEntity)
            .Where(feature => feature.QuizCategory.Id == categoryId);

        int totalFeatures = await query.CountAsync();

        if (totalFeatures == 0)
        {
            return null;
        }

        List<FeatureFloat> features = await query.OrderBy(feature => feature.Value).ToListAsync();

        List<FeatureFloat> subset;
        if (lastValue.HasValue)
        {
            int subsetSize = (int)Math.Ceiling(totalFeatures * coefficient);

            subset = features
                .OrderBy(feature => Math.Abs(feature.Value.Value - lastValue.Value))
                .Take(subsetSize)
                .ToList();
        }
        else
        {
            subset = features.ToList();
        }

        return subset.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
    }
}
