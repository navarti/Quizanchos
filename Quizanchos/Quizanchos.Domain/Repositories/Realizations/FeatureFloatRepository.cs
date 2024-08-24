using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class FeatureFloatRepository : EntityRepositoryBase<Guid, FeatureFloat>, IFeatureFloatRepository
{
    public FeatureFloatRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }
}
