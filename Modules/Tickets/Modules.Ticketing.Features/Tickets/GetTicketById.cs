using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Features.Tickets;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetTicketByIdHandler : IHandler
{
	Task<Result<TicketResponse>> HandleAsync(int ticketId, CancellationToken cancellationToken);
}

internal sealed class GetTicketByIdHandler(
		TicketingDbContext context,
		ILogger<GetTicketByIdHandler> logger)
		: IGetTicketByIdHandler
{
	public async Task<Result<TicketResponse>> HandleAsync(
			int ticketId,
			CancellationToken cancellationToken)
	{
		var ticket = await context.Tickets
				.AsNoTracking()
				.Where(t => t.Id == ticketId)
				.Select(t => new
				{
					t.Id,
					t.OtherTitle,
					ReferenceTitle = t.TicketTitle!.Name,
					t.Description,
					t.Status,
					t.CreatedAt,
					t.UpdatedAt
				})
				.FirstOrDefaultAsync(cancellationToken);

		if (ticket is null)
		{
			logger.LogInformation("Ticket with ID {TicketId} not found", ticketId);
			return Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");
		}

		logger.LogInformation("Retrieved ticket with ID: {TicketId}", ticketId);
		return new TicketResponse(
				ticket.Id,
				ticket.ReferenceTitle ?? ticket.OtherTitle,
				ticket.Description,
				ticket.Status.ToString(),
				ticket.CreatedAt,
				ticket.UpdatedAt);
	}
}
