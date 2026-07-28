using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Ticketing.Features.Ticket.Shared.Responses;
using Modules.Ticketing.Features.Ticket.Shared.Routes;

namespace Modules.Ticketing.Features.Ticket.GetTicketById;

public class GetTicketByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(TicketRoutes.GetTicketById, Handle)
            .RequireAuthorization("ticketing:view");
    }

    private static async Task<IResult> Handle(
        int ticketId,
        IGetTicketByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(ticketId, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
