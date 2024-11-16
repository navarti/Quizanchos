using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureIntRepository : IEntityRepository<Guid, FeatureInt>
{
    public Task<FeatureInt?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
}
