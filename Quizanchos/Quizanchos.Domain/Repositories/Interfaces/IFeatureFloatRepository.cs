using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureFloatRepository : IEntityRepository<Guid, FeatureFloat>
{
    public Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
}
