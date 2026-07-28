using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRM.SharedKernel.Infrastructure.Database;

public class EfRepository<T>(IApplicationDbContext dbContext)
    : IReadRepository<T>, IRepository<T> where T : class
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    public IQueryable<T> Query => _dbContext.Set<T>();

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellation = default)
    {
        try
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while adding the entity.", ex);
        }
    }

    public async Task<T> Add(T entity, CancellationToken cancellation = default)
    {
        try
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellation);
            return entity;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while adding the entity.", ex);
        }
    }

    public async Task Update(T entity, CancellationToken cancellation = default)
    {
        try
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync(cancellation);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while updating the entity.", ex);
        }
    }

    public async Task Delete(T entity, CancellationToken cancellation = default)
    {
        try
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync(cancellation);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while deleting the entity.", ex);
        }
    }

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellation = default)
    {
        return await _dbContext.Set<T>().FindAsync([id], cancellationToken: cancellation);
    }

    public async Task<T?> GetFirstOrDefaultAsync(IQueryable<T>? query = null, string[]? includes = null, CancellationToken cancellation = default)
    {
        query ??= Query;
        if (includes != null)
        {
            foreach (string include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync(cancellation);
    }

    public async Task<List<T>> GetListAsync(IQueryable<T>? query = null, string[]? includes = null, CancellationToken cancellation = default)
    {
        query ??= Query;
        if (includes != null)
        {
            foreach (string include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync(cancellation);
    }

    public async Task<TResult?> GetFirstOrDefaultAsync<TResult>(
    Expression<Func<T, TResult>> selector,
    IQueryable<T>? query = null,
    string[]? includes = null,
    CancellationToken cancellation = default)
    {
        query ??= Query;
        if (includes != null)
        {
            foreach (string include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.Select(selector).FirstOrDefaultAsync(cancellation);
    }

    public async Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        IQueryable<T>? query = null,
        string[]? includes = null,
        CancellationToken cancellation = default)
    {
        query ??= Query;
        if (includes != null)
        {
            foreach (string include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.Select(selector).ToListAsync(cancellation);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> condition, CancellationToken cancellation = default)
    {
        return Query.AnyAsync(condition, cancellation);
    }
    public Task<int> CountAsync(IQueryable<T>? query = null, CancellationToken cancellation = default)
    {
        query ??= Query;
        return query.CountAsync(cancellation);
    }
}

