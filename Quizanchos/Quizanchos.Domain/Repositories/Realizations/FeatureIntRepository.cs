using Microsoft.EntityFrameworkCore;
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

    public Task<FeatureInt?> FindRandomByCategory(Guid categoryId)
    {
        return _dbSet
            .Include(feature => feature.QuizEntity)
            .Where(feature => feature.QuizCategory.Id == categoryId)
            .OrderBy(feature => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }
}
