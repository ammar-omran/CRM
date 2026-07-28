using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Customers.Features.Customers.Shared.Routes;

namespace Modules.Customers.Features.Customers.GetCustomerById;

public class GetCustomerByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(CustomerRoutes.GetById, Handle)
            .RequireAuthorization("customers:view");
    }

    private static async Task<IResult> Handle(
        int customerId,
        IGetCustomerByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(customerId, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
