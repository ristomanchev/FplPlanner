using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ProektIntegrirani.Domain.Common;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Repository.Interface;

namespace ProektIntegrirani.Repository.Implementation;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public async Task<T> InsertAsync(T entity)
    {
        _entities.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<T>> InsertManyAsync(ICollection<T> entities)
    {
        _entities.AddRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _entities.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<T>> UpdateManyAsync(ICollection<T> entities)
    {
        _entities.UpdateRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<T> DeleteAsync(T entity)
    {
        _entities.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteManyAsync(ICollection<T> entities)
    {
        _entities.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _entities.AnyAsync(predicate);
    }

    public async Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        var query = BuildQuery(predicate, orderBy, include);
        return await query.Select(selector).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        int? take = null)
    {
        var query = BuildQuery(predicate, orderBy, include);
        if (take != null)
        {
            query = query.Take(take.Value);
        }

        return await query.Select(selector).ToListAsync();
    }

    public async Task<PaginatedResult<E>> GetAllPagedAsync<E>(Expression<Func<T, E>> selector,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        var query = BuildQuery(predicate, orderBy, include);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var projectedQuery = query.Select(selector);

        var totalCount = await projectedQuery.CountAsync();

        var items = await projectedQuery
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResult<E>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    private IQueryable<T> BuildQuery(Expression<Func<T, bool>>? predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include)
    {
        IQueryable<T> query = _entities;

        if (include != null)
        {
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return query;
    }
}
