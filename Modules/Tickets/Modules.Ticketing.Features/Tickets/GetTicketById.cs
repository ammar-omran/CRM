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

/// <summary>
/// Strong history DTO — raw field-level changes only.
/// Frontend is responsible for formatting/internationalization
/// (no TicketAuditHelper description strings here by design).
/// </summary>
public sealed record TicketHistoryResponse(
	int Id,
	string FieldName,
	string? OldValue,
	string NewValue,
	int OperatorId,
	string OperatorName,
	DateTime CreatedDate
);

public sealed record TicketOperatorResponse(int OperatorId, string Name, string Role);

/// <summary>
/// Detailed ticket view (mirrors old GetTicketDetailsDTO conventions
/// adapted to the new domain: category/type/severity + operators).
/// Comments/attachments intentionally excluded for now.
/// </summary>
public sealed record TicketDetailsResponse(
	int Id,
	string? Title,
	int? TitleId,
	string Description,
	string Status,
	int CategoryId,
	string Category,
	int TypeId,
	string Type,
	int? SeverityId,
	string? Severity,
	string? GroupId,
	IReadOnlyList<TicketOperatorResponse> Operators,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);

public interface IGetTicketByIdHandler : IHandler
{
	Task<Result<TicketDetailsResponse>> HandleAsync(
		int ticketId,
		CurrentUser currentUser,
		CancellationToken cancellationToken);
}

internal sealed class GetTicketByIdHandler(
		TicketingDbContext context,
		IHttpContextAccessor httpContextAccessor,
		IConfiguration configuration,
		ILogger<GetTicketByIdHandler> logger)
		: IGetTicketByIdHandler
{
	public async Task<Result<TicketDetailsResponse>> HandleAsync(
			int ticketId,
			CurrentUser currentUser,
			CancellationToken cancellationToken)
	{
		var ticket = await context.Tickets
				.AsNoTracking()
				.Where(t => t.Id == ticketId)
				.Select(t => new TicketDetailsResponse(
					t.Id,
					t.TicketTitle != null ? t.TicketTitle.Name : t.OtherTitle,
					t.TitleId,
					t.Description,
					t.Status.ToString(),
					t.CategoryId,
					t.Category.Name,
					t.TypeId,
					t.Type.Name,
					t.SeverityId,
					t.Severity != null ? t.Severity.Name : null,
					t.GroupId,
					t.TicketOperators
						.Select(to => new TicketOperatorResponse(to.OperatorId, to.Operator.Name, to.Role))
						.ToList(),
					t.CreatedAt,
					t.UpdatedAt))
				.FirstOrDefaultAsync(cancellationToken);

		if (ticket is null)
		{
			logger.LogInformation("Ticket with ID {TicketId} not found", ticketId);
			return Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");
		}

		// Full viewers pass; scoped callers must be assigned or share the group.
		var principal = httpContextAccessor.HttpContext?.User;
		if (!TicketAccess.HasFullView(principal))
		{
			var groupId = TicketQueryExtensions.ResolveGroupId(currentUser, configuration);
			var isMine = await TicketAccess.IsAssignedAsync(context, ticketId, currentUser.UserId, cancellationToken);

			if (!isMine && !TicketAccess.InGroup(ticket.GroupId, groupId))
			{
				logger.LogInformation("User {UserId} denied access to ticket {TicketId}", currentUser.UserId, ticketId);
				return TicketErrors.UnauthorizedAccess;
			}
		}

		logger.LogInformation("Retrieved ticket with ID: {TicketId}", ticketId);
		return ticket;
	}
}
