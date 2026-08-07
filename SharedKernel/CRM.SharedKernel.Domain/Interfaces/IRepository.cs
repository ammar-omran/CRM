using System.Linq.Expressions;

namespace CRM.SharedKernel.Domain.Interfaces;

public interface IReadRepository<T> where T : class
{
	IQueryable<T> Query { get; } // base query
	Task<List<T>> GetListAsync(IQueryable<T>? query = null, string[]? includes = null, CancellationToken cancellation = default);
	Task<T?> GetFirstOrDefaultAsync(IQueryable<T>? query = null, string[]? includes = null, CancellationToken cancellation = default);
	Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellation = default); // lazy loading

	Task<List<TResult>> GetListAsync<TResult>(
		Expression<Func<T, TResult>> selector,
		IQueryable<T>? query = null,
		string[]? includes = null,
		CancellationToken cancellation = default);
	Task<TResult?> GetFirstOrDefaultAsync<TResult>(
		Expression<Func<T, TResult>> selector,
		IQueryable<T>? query = null,
		string[]? includes = null,
		CancellationToken cancellation = default);
	Task<bool> AnyAsync(Expression<Func<T, bool>> condition,
		CancellationToken cancellation = default);
	Task<int> CountAsync(IQueryable<T>? query = null,
		CancellationToken cancellation = default);
}
public interface IRepository<T> : IReadRepository<T> where T : class
{
	new IQueryable<T> Query { get; } // base query
	Task<T> Add(T entity, CancellationToken cancellation = default);
	Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellation = default);
	Task Update(T entity, CancellationToken cancellation = default);
	Task Delete(T entity, CancellationToken cancellation = default);
}
