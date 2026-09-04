using CRM.SharedKernel.Application.API.Responses;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Features.Tickets.Shared;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetGroupTicketsHandler : IHandler
{
	Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CurrentUser currentUser,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// GET api/tickets/group — tickets sharing the caller's group.
/// Group id is resolved from the token payload via the configured GroupKey
/// (same convention as ticket creation): ticket.GroupId == payload[GroupKey].
/// </summary>
internal sealed class GetGroupTicketsHandler(
	TicketingDbContext dbContext,
	IConfiguration configuration,
	ILogger<GetGroupTicketsHandler> logger) : IGetGroupTicketsHandler
{
	public async Task<Result<PaginationResponse<TicketResponse>>> HandleAsync(
		TicketFilterRequest request,
		CurrentUser currentUser,
		CancellationToken ct = default)
	{
		var groupId = TicketQueryExtensions.ResolveGroupId(currentUser, configuration);

		// No group claim (or unconfigured GroupKey): empty page, not an error.
		// Mirrors the old agent-scoped behavior which returned an empty list
		// when the caller had no customers in scope.
		if (string.IsNullOrWhiteSpace(groupId))
		{
			logger.LogInformation("No group claim for user {UserId}; returning empty group list", currentUser.UserId);
			var emptyPage = request.Limit > 0 ? request.Skip / request.Limit : 0;
			return new PaginationResponse<TicketResponse>([], emptyPage, request.Limit, 0);
		}

		var query = dbContext.Tickets
			.AsNoTracking()
			.Where(t => t.GroupId == groupId)
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

		logger.LogInformation("Retrieved {Count} tickets for group {GroupId}", items.Count, groupId);

		return new PaginationResponse<TicketResponse>(items, request.Skip / request.Limit, request.Limit, total);
	}
}
