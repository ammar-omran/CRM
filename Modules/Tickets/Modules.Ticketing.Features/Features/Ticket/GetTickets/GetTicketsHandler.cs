using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Features.Ticket.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Modules.Ticketing.Features.Ticket.GetTickets;

public interface IGetTicketsHandler : IHandler
{
    Task<Result<IReadOnlyList<TicketResponse>>> HandleAsync(CancellationToken cancellationToken = default);
}

internal sealed class GetTicketsHandler : IGetTicketsHandler
{
    private readonly TicketingDbContext _dbContext;

    public GetTicketsHandler(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<TicketResponse>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new TicketResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Status.ToString(),
                t.CustomerEmail,
                t.CustomerName,
                t.CreatedByName,
                t.CreatedDate,
                t.UpdatedDate))
            .ToListAsync(cancellationToken);

        return tickets;
    }
}
