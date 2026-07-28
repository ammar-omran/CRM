using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Extensions;
using Modules.Organizations.Features.Agent.Shared.Requests;
using Modules.Organizations.Features.Agent.Shared.Routes;

namespace Modules.Organizations.Features.Agent.GetAgents;

public class GetAgentsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(RouteConsts.BaseRoute, Handle);
    }

    private static async Task<IResult> Handle(
        [AsParameters] AgentByRequest request,
        IValidator<AgentByRequest> validator,
        IGetAgentsHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
            return EndpointResultsExtensions.ToProblem(response.Errors);

        return Results.Ok(response.Value);
    }
}
