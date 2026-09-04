using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Features.Tickets.Shared;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetTicketsListHandler : IHandler
{
	Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// GET api/tickets/list — unscoped list (policy-gated, e.g. supervisor/admin).
/// Supports pagination + filtering. No agent/group isolation here by design.
/// </summary>
internal sealed class GetTicketsListHandler(
	TicketingDbContext dbContext,
	ILogger<GetTicketsListHandler> logger) : IGetTicketsListHandler
{
	public async Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CancellationToken ct = default)
	{
		var query = dbContext.Tickets
			.AsNoTracking()
			.ApplyFilters(request);

		var total = request.SkipTotal ? -1 : await query.CountAsync(ct);

		var items = await query
			.OrderByDescending(t => t.CreatedAt)
			.Skip(request.Skip)
			.Take(request.Limit)
			.Select(t => new TicketResponse(
				t.Id,
				t.TicketTitle != null ? t.TicketTitle.Name : t.OtherTitle,
				t.Description,
				t.Status.ToString(),
				t.Severity != null ? t.Severity.Name : null,
				t.Category.Name,
				t.Type.Name,
				t.GroupId,
				t.CreatedAt,
				t.UpdatedAt))
			.ToListAsync(ct);

		logger.LogInformation("Retrieved {Count} tickets (list)", items.Count);

		return new PaginationResponse<TicketResponse>(items, request.Skip / request.Limit, request.Limit, total);
	}
}
