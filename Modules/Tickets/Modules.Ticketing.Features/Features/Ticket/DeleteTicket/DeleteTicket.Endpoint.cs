using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using Modules.Ticketing.Features.Ticket.Shared.Routes;

namespace Modules.Ticketing.Features.Ticket.DeleteTicket;

public class DeleteTicketEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(TicketRoutes.DeleteTicket, Handle)
            .RequireAuthorization("ticketing:delete");
    }

    private static async Task<IResult> Handle(
        int ticketId,
        IDeleteTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(ticketId, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}
