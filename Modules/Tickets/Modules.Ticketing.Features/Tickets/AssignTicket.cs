using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Domain.Errors;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public sealed record AssignTicketRequest(
	string AssigneeRefId,
	string? AssigneeName = null,
	string? AssigneeEmail = null,
	TicketOperationRole Role = TicketOperationRole.Assignee);

public sealed record AssignTicketResponse(
	int TicketId,
	int OperatorId,
	string AssigneeRefId,
	string Role);

public interface IAssignTicketHandler : IHandler
{
	Task<Result<AssignTicketResponse>> HandleAsync(
		int ticketId,
		AssignTicketRequest request,
		CurrentUser currentUser,
		CancellationToken cancellationToken);
}

internal sealed class AssignTicketHandler(
	TicketingDbContext context,
	ILogger<AssignTicketHandler> logger) : IAssignTicketHandler
{
	public async Task<Result<AssignTicketResponse>> HandleAsync(
		int ticketId,
		AssignTicketRequest request,
		CurrentUser currentUser,
		CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(request.AssigneeRefId))
			return Error.Validation("Ticket.AssigneeRequired", "AssigneeRefId is required.");

		var ticket = await context.Tickets
			.Include(t => t.TicketOperators)
			.FirstOrDefaultAsync(t => t.Id == ticketId, ct);

		if (ticket is null)
		{
			logger.LogInformation("Ticket {TicketId} not found for assignment", ticketId);
			return TicketErrors.NotFound(ticketId);
		}

		var actorResult = await GetOrCreateOperatorAsync(currentUser, ct);
		if (actorResult.IsError)
			return actorResult.Errors;

		var actor = actorResult.Value!;

		var assignee = await context.Operators
			.FirstOrDefaultAsync(o => o.RefId == request.AssigneeRefId, ct);

		if (assignee is null)
		{
			assignee = new Operator
			{
				RefId = request.AssigneeRefId,
				Name = request.AssigneeName ?? string.Empty,
				Email = request.AssigneeEmail ?? string.Empty,
			};
			context.Operators.Add(assignee);
			await context.SaveChangesAsync(ct);
		}
		else
		{
			// Keep stored profile fresh when caller supplies newer details.
			var dirty = false;
			if (!string.IsNullOrWhiteSpace(request.AssigneeName) && assignee.Name != request.AssigneeName)
			{
				assignee.Name = request.AssigneeName!;
				dirty = true;
			}
			if (!string.IsNullOrWhiteSpace(request.AssigneeEmail) && assignee.Email != request.AssigneeEmail)
			{
				assignee.Email = request.AssigneeEmail!;
				dirty = true;
			}
			if (dirty)
				await context.SaveChangesAsync(ct);
		}

		// Role is dynamic and lives only on TicketOperator as "{operation}:{userCsv}".
		// UserRoles part is empty here — assignee's current roles are not snapshotted on Operator.
		var error = ticket.AssignOperator(assignee, request.Role, actor, null);
		if (error is not null)
			return error.Value;

		await context.SaveChangesAsync(ct);
		logger.LogInformation("Ticket {TicketId} assigned to {AssigneeRefId} ({Role}) by {Actor}", ticketId, assignee.RefId, request.Role, actor.RefId);

		return new AssignTicketResponse(ticket.Id, assignee.Id, assignee.RefId, ticket.TicketOperators.First(o => o.OperatorId == assignee.Id).Role);
	}

	private async Task<Result<Operator>> GetOrCreateOperatorAsync(CurrentUser currentUser, CancellationToken ct)
	{
		if (currentUser.UserId is null)
			return Error.Unauthorized("Auth.IdentityRequired", "A valid user identity is required to assign a ticket.");

		var op = await context.Operators.FirstOrDefaultAsync(o => o.RefId == currentUser.UserId, ct);
		if (op is not null)
		{
			if (op.Name != (currentUser.Name ?? string.Empty) || op.Email != (currentUser.Email ?? string.Empty))
			{
				if (!string.IsNullOrWhiteSpace(currentUser.Name)) op.Name = currentUser.Name!;
				if (!string.IsNullOrWhiteSpace(currentUser.Email)) op.Email = currentUser.Email!;
				await context.SaveChangesAsync(ct);
			}
			return op;
		}

		op = new Operator
		{
			RefId = currentUser.UserId,
			Name = currentUser.Name ?? string.Empty,
			Email = currentUser.Email ?? string.Empty,
		};
		context.Operators.Add(op);
		await context.SaveChangesAsync(ct);
		return op;
	}
}
