using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Users.Shared.Routes;

namespace Modules.Users.Features.Users.GetUserById;

public class GetUserByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(RouteConsts.GetById, Handle)
            .RequireAuthorization(UserPolicyConstants.ReadPolicy);
    }

    private static async Task<IResult> Handle(
        string userId,
        IGetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(userId, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
