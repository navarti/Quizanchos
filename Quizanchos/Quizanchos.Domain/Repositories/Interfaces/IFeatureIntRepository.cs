using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureIntRepository : IEntityRepository<Guid, FeatureInt>
{
    Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
    Task<FeatureInt?> FindRandomByCategory(Guid categoryId);
}
