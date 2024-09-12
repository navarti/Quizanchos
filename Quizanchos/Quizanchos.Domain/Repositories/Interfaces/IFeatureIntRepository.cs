using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureIntRepository : IEntityRepository<Guid, FeatureInt>
{
    public Task<FeatureInt> GetByCategoryAndEntity(Guid categoryId, Guid entityId);
}
