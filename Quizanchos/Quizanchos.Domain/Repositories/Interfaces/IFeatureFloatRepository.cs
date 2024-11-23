using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IFeatureFloatRepository : IEntityRepository<Guid, FeatureFloat>
{
    Task<FeatureFloat?> FindByCategoryAndEntity(Guid categoryId, Guid entityId);
    Task<FeatureFloat?> FindRandomByCategory(Guid categoryId);
}
