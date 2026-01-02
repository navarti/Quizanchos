using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureIntRepository : IEntityRepository<Guid, FeatureInt>
{
    Task<FeatureInt> GetByIdIncluding(Guid id);
    Task<List<FeatureInt>> GetByCategoryId(Guid categoryId);
    Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
    Task<FeatureInt?> FindRandomByCategory(Guid categoryId, double coefficient, int? lastValue = null);
}
