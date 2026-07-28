using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Features.Ticket.Shared.Responses;
using Modules.Ticketing.Infrastructure.Database;
using TicketManagement.Domain.Entities;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace Modules.Ticketing.Features.Ticket.GetTicketById;

internal interface IGetTicketByIdHandler : IHandler
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
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            logger.LogInformation("Ticket with ID {TicketId} not found", ticketId);
            return CRM.SharedKernel.Domain.Results.Error.NotFound("Ticket.NotFound", $"Ticket with ID {ticketId} was not found.");
        }

        logger.LogInformation("Retrieved ticket with ID: {TicketId}", ticketId);
        return new TicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.CustomerEmail,
            ticket.CustomerName,
            ticket.CreatedByName,
            ticket.CreatedDate,
            ticket.UpdatedDate);
    }
}
