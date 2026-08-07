using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Infrastructure.Database;
using TicketManagement.Domain.Entities;

namespace Modules.Ticketing.Features.Ticket.UpdateTicket;

public sealed record UpdateTicketRequest(
		int Id,
		string Title,
		string Description,
		string Status
);

public sealed record UpdateTicketResponse(int Id, string Title, string Status);

internal interface IUpdateTicketHandler : IHandler
{
	Task<Result<UpdateTicketResponse>> HandleAsync(UpdateTicketRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateTicketHandler(
		TicketingDbContext context,
		ILogger<UpdateTicketHandler> logger)
		: IUpdateTicketHandler
{
	public async Task<Result<UpdateTicketResponse>> HandleAsync(
			UpdateTicketRequest request,
			CancellationToken cancellationToken)
	{
		var ticket = await context.Tickets
				.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

		if (ticket is null)
		{
			logger.LogInformation("Ticket with ID {TicketId} not found", request.Id);
			return CRM.SharedKernel.Domain.Results.Error.NotFound("Ticket.NotFound", $"Ticket with ID {request.Id} was not found.");
		}

		ticket.Title = request.Title;
		ticket.Description = request.Description;
		ticket.UpdatedDate = DateTime.UtcNow;

		if (Enum.TryParse<TicketStatusEnum>(request.Status, true, out var status))
		{
			ticket.Status = status;
		}

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Ticket updated: {TicketId}", ticket.Id);
		return new UpdateTicketResponse(ticket.Id, ticket.Title, ticket.Status.ToString());
	}
}
