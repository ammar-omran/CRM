using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Customers.Features.Customers.Shared.Routes;

namespace Modules.Customers.Features.Customers.Register;

public class RegisterEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(CustomerRoutes.Register, Handle)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        RegisterRequest request,
        IRegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Created($"/api/customers/{response.Value?.Id}", response.Value);
    }
}
