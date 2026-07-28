using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Customers.Features.Customers.Shared.Routes;

namespace Modules.Customers.Features.Customers.Login;

public class LoginEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(CustomerRoutes.Login, Handle)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        LoginRequest request,
        ILoginHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
