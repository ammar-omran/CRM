using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Domain.Entities;
using TicketEntity = TicketManagement.Domain.Entities;

namespace Modules.Ticketing.Features.Ticket.CreateTicket;

public sealed record CreateTicketRequest(
		string Title,
		string Description,
		string CustomerEmail,
		string CustomerName,
		int CategoryId,
		int TypeId
);

public sealed record CreateTicketResponse(int Id, string Title, string Status);

internal interface ICreateTicketHandler : IHandler
{
	Task<Result<CreateTicketResponse>> HandleAsync(CreateTicketRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateTicketHandler(
		TicketingDbContext context,
		ILogger<CreateTicketHandler> logger)
		: ICreateTicketHandler
{
	public async Task<Result<CreateTicketResponse>> HandleAsync(
			CreateTicketRequest request,
			CancellationToken cancellationToken)
	{
		var ticket = new TicketEntity.Ticket
		{
			Title = request.Title,
			Description = request.Description,
			CustomerEmail = request.CustomerEmail,
			CustomerName = request.CustomerName,
			CategoryId = request.CategoryId,
			TypeId = request.TypeId,
			Status = TicketStatusEnum.Open,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow,
			CreatedBy = 0,
			CreatedByName = "System"
		};

		context.Tickets.Add(ticket);
		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Ticket created with ID: {TicketId}", ticket.Id);
		return new CreateTicketResponse(ticket.Id, ticket.Title, ticket.Status.ToString());
	}
}
