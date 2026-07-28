using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Ticketing.Features.Ticket.Shared.Responses;
using Modules.Ticketing.Features.Ticket.Shared.Routes;

namespace Modules.Ticketing.Features.Ticket.GetTickets;

public sealed class GetTicketsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(TicketRoutes.GetTickets, async (IGetTicketsHandler handler) =>
        {
            var response = await handler.HandleAsync();

            if (response.IsError)
            {
                return response.Errors.ToProblem();
            }

            return Results.Ok(response.Value);
        })
        .WithName("GetTickets")
        .Produces<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)
        .WithTags("Tickets");
    }
}
