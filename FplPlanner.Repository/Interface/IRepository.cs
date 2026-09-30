using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using FplPlanner.Domain.Common;
using FplPlanner.Domain.Dto;

namespace FplPlanner.Repository.Interface;

public interface IRepository<T> where T : BaseEntity
{
    Task<T> InsertAsync(T entity);
    Task<ICollection<T>> InsertManyAsync(ICollection<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<ICollection<T>> UpdateManyAsync(ICollection<T> entities);
    Task<T> DeleteAsync(T entity);
    Task DeleteManyAsync(ICollection<T> entities);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

    Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        int? take = null);

    Task<PaginatedResult<E>> GetAllPagedAsync<E>(Expression<Func<T, E>> selector,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false);
}
