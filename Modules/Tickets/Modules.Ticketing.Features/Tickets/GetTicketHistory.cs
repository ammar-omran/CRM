using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Domain.Errors;
using Modules.Ticketing.Features.Tickets.Shared;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetTicketHistoryHandler : IHandler
{
	Task<Result<IReadOnlyList<TicketHistoryResponse>>> HandleAsync(
		int ticketId,
		CurrentUser currentUser,
		CancellationToken cancellationToken);
}

/// <summary>
/// GET api/tickets/{id}/history — raw audit trail for a ticket.
/// Returns strong DTOs (field/old/new/actor/timestamp); no formatted
/// Title/Description strings so the frontend can localize rendering.
/// Same scoped-access rules as single-ticket reads.
/// </summary>
internal sealed class GetTicketHistoryHandler(
	TicketingDbContext context,
	IHttpContextAccessor httpContextAccessor,
	IConfiguration configuration,
	ILogger<GetTicketHistoryHandler> logger) : IGetTicketHistoryHandler
{
	public async Task<Result<IReadOnlyList<TicketHistoryResponse>>> HandleAsync(
		int ticketId,
		CurrentUser currentUser,
		CancellationToken ct)
	{
		var ticket = await context.Tickets
			.AsNoTracking()
			.Where(t => t.Id == ticketId)
			.Select(t => new { t.Id, t.GroupId })
			.FirstOrDefaultAsync(ct);

		if (ticket is null)
		{
			logger.LogInformation("Ticket with ID {TicketId} not found for history", ticketId);
			return Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");
		}

		var principal = httpContextAccessor.HttpContext?.User;
		if (!TicketAccess.HasFullView(principal))
		{
			var groupId = TicketQueryExtensions.ResolveGroupId(currentUser, configuration);
			var isMine = await TicketAccess.IsAssignedAsync(context, ticketId, currentUser.UserId, ct);

			if (!isMine && !TicketAccess.InGroup(ticket.GroupId, groupId))
			{
				logger.LogInformation("User {UserId} denied history access to ticket {TicketId}", currentUser.UserId, ticketId);
				return TicketErrors.UnauthorizedAccess;
			}
		}

		var history = await context.TicketHistories
			.AsNoTracking()
			.Where(h => h.TicketId == ticketId)
			.OrderByDescending(h => h.CreatedDate)
			.Select(h => new TicketHistoryResponse(
				h.Id,
				h.FieldName,
				h.OldValue,
				h.NewValue,
				h.OperatorId,
				h.OperatorName,
				h.CreatedDate))
			.ToListAsync(ct);

		return history;
	}
}
