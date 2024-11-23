using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Enums;
using System.Linq.Expressions;

using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Entities.Interfaces;
using Quizanchos.Common.Util;

namespace Quizanchos.Domain.Repositories.Realizations;

public class EntityRepositoryBase<TKey, TEntity> : IEntityRepository<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>
{
    protected readonly QuizDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public EntityRepositoryBase(QuizDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity> Create(TEntity entity)
    {
        await _dbSet.AddAsync(entity).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public virtual async Task Delete(TEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Deleted;
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        Expression<Func<TEntity, bool>>? whereExpression = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection>? orderBy = null,
        bool asNoTracking = false)
    {
        IQueryable<TEntity> query = _dbSet;
        if (whereExpression != null)
        {
            query = query.Where(whereExpression);
        }

        if (orderBy != null && orderBy.Any())
        {
            var orderedData = orderBy.Values.First() == SortDirection.Ascending
                ? query.OrderBy(orderBy.Keys.First())
                : query.OrderByDescending(orderBy.Keys.First());

            foreach (var expression in orderBy.Skip(1))
            {
                orderedData = expression.Value == SortDirection.Ascending
                    ? orderedData.ThenBy(expression.Key)
                    : orderedData.ThenByDescending(expression.Key);
            }

            query = orderedData;
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take > 0)
        {
            query = query.Take(take);
        }

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public virtual async Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> whereExpression)
    {
        var query = _dbSet.Where(whereExpression);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<TEntity> GetById(TKey id) => await FindById(id) ?? throw HandledExceptionFactory.CreateIdNotFoundException(id);

    public virtual Task<TEntity?> FindById(TKey id) => _dbSet.FirstOrDefaultAsync(x => x.Id != null && x.Id.Equals(id));

    public virtual async Task<TEntity> Update(TEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public async Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        var query = _dbSet.FirstOrDefaultAsync(whereExpression);
        return await query.ConfigureAwait(false);
    }
}
