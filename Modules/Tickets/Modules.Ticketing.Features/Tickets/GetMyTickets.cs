using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Features.Tickets.Shared;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetMyTicketsHandler : IHandler
{
	Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CurrentUser currentUser,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// GET api/tickets/mine — tickets where the current user is an assigned operator
/// (TicketOperators → Operator.RefId == CurrentUser.UserId).
/// Replaces the old hardcoded Agent-scoped filtering with policy isolation.
/// </summary>
internal sealed class GetMyTicketsHandler(
	TicketingDbContext dbContext,
	ILogger<GetMyTicketsHandler> logger) : IGetMyTicketsHandler
{
	public async Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CurrentUser currentUser,
		CancellationToken ct = default)
	{
		if (currentUser.UserId is null)
			return Error.Unauthorized("Auth.IdentityRequired", "A valid user identity is required.");

		var userId = currentUser.UserId;

		var query = dbContext.Tickets
			.AsNoTracking()
			.Where(t => t.TicketOperators.Any(to => to.Operator.RefId == userId))
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

		logger.LogInformation("Retrieved {Count} tickets for user {UserId} (mine)", items.Count, userId);

		return new PaginationResponse<TicketResponse>(items, request.Skip / request.Limit, request.Limit, total);
	}
}
