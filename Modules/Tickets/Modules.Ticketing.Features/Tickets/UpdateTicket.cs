using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Features.Tickets;

public sealed record UpdateTicketRequest(
		int Id,
		string Title,
		string Description,
		string Status
);

public sealed record UpdateTicketResponse(int Id, string? Title, string Status);

public interface IUpdateTicketHandler : IHandler
{
	Task<Result<UpdateTicketResponse>> HandleAsync(UpdateTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
}

internal sealed class UpdateTicketHandler(
		TicketingDbContext context,
		ILogger<UpdateTicketHandler> logger)
		: IUpdateTicketHandler
{
	public async Task<Result<UpdateTicketResponse>> HandleAsync(
			UpdateTicketRequest request,
			CurrentUser currentUser,
			CancellationToken cancellationToken)
	{
		var ticket = await context.Tickets
				.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

		if (ticket is null)
		{
			logger.LogInformation("Ticket with ID {TicketId} not found", request.Id);
			return Error.NotFound("Ticket.NotFound", $"Ticket with ID {request.Id} was not found.");
		}

		var operatorResult = await GetOrCreateOperatorAsync(currentUser, cancellationToken);
		if (operatorResult.IsError)
		{
			return operatorResult.Errors;
		}

		var actor = operatorResult.Value!;

		var errors = new List<Error>();
		var titleError = ticket.SetOtherTitle(request.Title, actor);
		if (titleError is not null)
		{
			errors.Add(titleError.Value);
		}

		var descriptionError = ticket.SetDescription(request.Description, actor);
		if (descriptionError is not null)
		{
			errors.Add(descriptionError.Value);
		}

		if (Enum.TryParse<TicketStatusEnum>(request.Status, true, out var status))
		{
			var statusError = ticket.ChangeStatus(status, actor);
			if (statusError is not null)
			{
				errors.Add(statusError.Value);
			}
		}

		if (errors.Count > 0)
		{
			return errors;
		}

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Ticket updated: {TicketId}", ticket.Id);
		return new UpdateTicketResponse(ticket.Id, ticket.Title, ticket.Status.ToString());
	}

	private async Task<Result<Operator>> GetOrCreateOperatorAsync(
		CurrentUser currentUser,
		CancellationToken cancellationToken)
	{
		if (currentUser.UserId is null)
		{
			return Error.Unauthorized("Auth.IdentityRequired", "A valid user identity is required to update a ticket.");
		}

		var @operator = await context.Operators
			.FirstOrDefaultAsync(o => o.RefId == currentUser.UserId.Value, cancellationToken);

		if (@operator is not null)
		{
			return @operator;
		}

		@operator = new Operator
		{
			RefId = currentUser.UserId.Value,
			Name = currentUser.Name ?? string.Empty,
			Email = currentUser.Email ?? string.Empty
		};

		context.Operators.Add(@operator);
		await context.SaveChangesAsync(cancellationToken);

		return @operator;
	}
}
