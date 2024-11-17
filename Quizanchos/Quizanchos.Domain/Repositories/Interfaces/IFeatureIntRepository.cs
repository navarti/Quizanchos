using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureIntRepository : IEntityRepository<Guid, FeatureInt>
{
    public Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
}
