using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Domain.Entities;
using TicketEntity = Modules.Ticketing.Domain.Entities.Ticket;
using Microsoft.Extensions.Configuration;

namespace Modules.Ticketing.Features.Tickets;

public sealed record CreateTicketRequest(
	string? OtherTitle,
	string Description,
	int CategoryId,
	int TypeId,
	int? TitleId = null,
	int? SeverityId = null
);

public sealed record CreateTicketResponse(int Id, string? Title, string Status);

public interface ICreateTicketHandler : IHandler
{
	Task<Result<CreateTicketResponse>> HandleAsync(
		CreateTicketRequest request,
		CurrentUser currentUser,
		CancellationToken cancellationToken);
}

internal sealed class CreateTicketHandler(
	TicketingDbContext context,
	ILogger<CreateTicketHandler> logger,
	IConfiguration configurations)
	: ICreateTicketHandler
{
	public async Task<Result<CreateTicketResponse>> HandleAsync(
		CreateTicketRequest request,
		CurrentUser currentUser,
		CancellationToken cancellationToken)
	{
		var category = await context.Categories
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

		if (category is null)
		{
			return Error.NotFound("Category.NotFound", $"Category with ID {request.CategoryId} was not found.");
		}

		var type = await context.TicketTypes
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == request.TypeId, cancellationToken);

		if (type is null)
		{
			return Error.NotFound("TicketType.NotFound", $"Ticket type with ID {request.TypeId} was not found.");
		}

		TicketTitle? ticketTitle = null;
		if (request.TitleId is not null)
		{
			ticketTitle = await context.TicketTitles
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.Id == request.TitleId, cancellationToken);

			if (ticketTitle is null)
			{
				return Error.NotFound("TicketTitle.NotFound", $"Ticket title with ID {request.TitleId} was not found.");
			}
		}

		Severity? severity = null;
		if (request.SeverityId is not null)
		{
			severity = await context.Severities
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.Id == request.SeverityId, cancellationToken);

			if (severity is null)
			{
				return Error.NotFound("Severity.NotFound", $"Severity with ID {request.SeverityId} was not found.");
			}
		}

		var operatorResult = await GetOrCreateOperatorAsync(currentUser, cancellationToken);
		if (operatorResult.IsError)
		{
			return operatorResult.Errors;
		}

		var groupKey = configurations["GroupKey"] as string;
		currentUser.TokenPayload.TryGetValue(groupKey, out var groupId);

		var createResult = TicketEntity.Create(
			request.OtherTitle,
			request.Description,
			category,
			type,
			operatorResult.Value!,
			currentUser.Role,
			severity,
			ticketTitle,
			groupId.ToString());

		if (createResult.IsError)
		{
			return createResult.Errors;
		}

		var ticket = createResult.Value!;
		context.Tickets.Add(ticket);
		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Ticket created with ID: {TicketId}", ticket.Id);
		return new CreateTicketResponse(ticket.Id, ticket.Title, ticket.Status.ToString());
	}

	private async Task<Result<Operator>> GetOrCreateOperatorAsync(
		CurrentUser currentUser,
		CancellationToken cancellationToken)
	{
		if (currentUser.UserId is null)
		{
			return Error.Unauthorized("Auth.IdentityRequired", "A valid user identity is required to create a ticket.");
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
