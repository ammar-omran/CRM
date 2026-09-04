using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

/// <summary>Generic id/name option for the ticket create form (old LookupItemDTO port).</summary>
public sealed record LookupItemResponse(int Id, string Name);

/// <summary>Title option scoped to a category (old TicketTitleDTO port).</summary>
public sealed record TicketTitleLookupResponse(int Id, string Name, int CategoryId, int? DefaultSeverityId);

public interface ITicketLookupsHandler : IHandler
{
	Task<Result<IReadOnlyList<LookupItemResponse>>> GetSeveritiesAsync(CancellationToken ct = default);
	Task<Result<IReadOnlyList<LookupItemResponse>>> GetCategoriesAsync(CancellationToken ct = default);
	Task<Result<IReadOnlyList<LookupItemResponse>>> GetTypesAsync(CancellationToken ct = default);
	Task<Result<IReadOnlyList<LookupItemResponse>>> GetServicesAsync(CancellationToken ct = default);
	Task<Result<IReadOnlyList<LookupItemResponse>>> GetStatusesAsync(CancellationToken ct = default);
	Task<Result<IReadOnlyList<TicketTitleLookupResponse>>> GetTitlesAsync(int? categoryId, CancellationToken ct = default);
}

/// <summary>
/// Reference data for the ticket create form and list filters.
/// Only visible entries, ordered by Sort then Name. Statuses come from
/// the TicketStatusEnum (no table); titles can be narrowed by category.
/// </summary>
internal sealed class TicketLookupsHandler(
	TicketingDbContext dbContext,
	ILogger<TicketLookupsHandler> logger) : ITicketLookupsHandler
{
	public async Task<Result<IReadOnlyList<LookupItemResponse>>> GetSeveritiesAsync(CancellationToken ct = default)
	{
		var items = await dbContext.Severities
			.AsNoTracking()
			.Where(s => s.IsVisible)
			.OrderBy(s => s.Sort)
			.ThenBy(s => s.Name)
			.Select(s => new LookupItemResponse(s.Id, s.Name))
			.ToListAsync(ct);
		return items;
	}

	public async Task<Result<IReadOnlyList<LookupItemResponse>>> GetCategoriesAsync(CancellationToken ct = default)
	{
		var items = await dbContext.Categories
			.AsNoTracking()
			.Where(c => c.IsVisible)
			.OrderBy(c => c.Sort)
			.ThenBy(c => c.Name)
			.Select(c => new LookupItemResponse(c.Id, c.Name))
			.ToListAsync(ct);
		return items;
	}

	public async Task<Result<IReadOnlyList<LookupItemResponse>>> GetTypesAsync(CancellationToken ct = default)
	{
		var items = await dbContext.TicketTypes
			.AsNoTracking()
			.Where(t => t.IsVisible)
			.OrderBy(t => t.Sort)
			.ThenBy(t => t.Name)
			.Select(t => new LookupItemResponse(t.Id, t.Name))
			.ToListAsync(ct);
		return items;
	}

	public async Task<Result<IReadOnlyList<LookupItemResponse>>> GetServicesAsync(CancellationToken ct = default)
	{
		var items = await dbContext.Set<Service>()
			.AsNoTracking()
			.Where(s => s.IsVisible)
			.OrderBy(s => s.Sort)
			.ThenBy(s => s.Name)
			.Select(s => new LookupItemResponse(s.Id, s.Name))
			.ToListAsync(ct);
		return items;
	}

	public Task<Result<IReadOnlyList<LookupItemResponse>>> GetStatusesAsync(CancellationToken ct = default)
	{
		List<LookupItemResponse> items = Enum.GetValues<TicketStatusEnum>()
			.Select(s => new LookupItemResponse((int)s, s.ToString()))
			.ToList();
		Result<IReadOnlyList<LookupItemResponse>> result = items;
		return Task.FromResult(result);
	}

	public async Task<Result<IReadOnlyList<TicketTitleLookupResponse>>> GetTitlesAsync(int? categoryId, CancellationToken ct = default)
	{
		var query = dbContext.TicketTitles.AsNoTracking();

		if (categoryId.HasValue)
			query = query.Where(t => t.CategoryId == categoryId.Value);

		var items = await query
			.OrderBy(t => t.Name)
			.Select(t => new TicketTitleLookupResponse(t.Id, t.Name, t.CategoryId, t.DefaultSeverityId))
			.ToListAsync(ct);

		logger.LogInformation("Retrieved {Count} ticket titles (categoryId: {CategoryId})", items.Count, categoryId);
		return items;
	}
}
