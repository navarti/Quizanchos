using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureFloatRepository : IEntityRepository<Guid, FeatureFloat>
{
    public Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
}
