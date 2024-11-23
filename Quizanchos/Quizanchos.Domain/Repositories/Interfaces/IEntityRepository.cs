using System.Linq.Expressions;
using Quizanchos.Common.Enums;

namespace Quizanchos.Domain.Repositories.Interfaces;

public interface IEntityRepository<TKey, TEntity>
{
    Task<TEntity> Create(TEntity entity);

    IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection>? orderBy = null,
        bool asNoTracking = false);

    Task<TEntity> GetById(TKey id);

    Task<TEntity?> FindById(TKey id);

    Task<TEntity> Update(TEntity entity);

    Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> whereExpression);

    Task Delete(TEntity entity);

    Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> whereExpression);
}
