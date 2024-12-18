using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureFloatRepository : IEntityRepository<Guid, FeatureFloat>
{
    Task<FeatureFloat> GetByIdIncluding(Guid id);
    Task<List<FeatureFloat>> GetByCategoryId(Guid categoryId);
    Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
    Task<FeatureFloat?> FindRandomByCategory(Guid categoryId, double coefficient, float? lastValue = null);
}
