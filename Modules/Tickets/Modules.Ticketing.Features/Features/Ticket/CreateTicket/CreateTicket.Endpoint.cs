using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using Modules.Ticketing.Features.Ticket.Shared.Routes;

namespace Modules.Ticketing.Features.Ticket.CreateTicket;

public class CreateTicketEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(TicketRoutes.CreateTicket, Handle)
            .RequireAuthorization("ticketing:create");
    }

    private static async Task<IResult> Handle(
        CreateTicketRequest request,
        ICreateTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Created($"/api/tickets/{response.Value?.Id}", response.Value);
    }
}
