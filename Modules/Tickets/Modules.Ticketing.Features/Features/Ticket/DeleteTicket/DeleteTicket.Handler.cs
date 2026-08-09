using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Domain.Entities;

namespace Modules.Ticketing.Features.Ticket.DeleteTicket;

internal interface IDeleteTicketHandler : IHandler
{
    Task<Result<bool>> HandleAsync(int ticketId, CancellationToken cancellationToken);
}

internal sealed class DeleteTicketHandler(
    TicketingDbContext context,
    ILogger<DeleteTicketHandler> logger)
    : IDeleteTicketHandler
{
    public async Task<Result<bool>> HandleAsync(
        int ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            logger.LogInformation("Ticket with ID {TicketId} not found for deletion", ticketId);
            return CRM.SharedKernel.Domain.Results.Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");
        }

        if (ticket.Status == TicketStatusEnum.Closed)
        {
            return CRM.SharedKernel.Domain.Results.Error.Failure("Ticket.CannotDeleteClosedTicket", "Cannot delete a closed ticket.");
        }

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ticket deleted: {TicketId}", ticketId);
        return true;
    }
}
