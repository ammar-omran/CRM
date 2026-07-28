using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Ticketing.Features.Ticket.Shared.Routes;

namespace Modules.Ticketing.Features.Ticket.UpdateTicket;

public class UpdateTicketEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(TicketRoutes.UpdateTicket, Handle)
            .RequireAuthorization("ticketing:update");
    }

    private static async Task<IResult> Handle(
        int ticketId,
        UpdateTicketRequest request,
        IUpdateTicketHandler handler,
        CancellationToken cancellationToken)
    {
        if (ticketId != request.Id)
        {
            return Results.BadRequest(new { Error = "Ticket ID mismatch" });
        }

        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
