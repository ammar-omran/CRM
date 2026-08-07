using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using Modules.Users.Features.Organization.Shared.Requests;
using Modules.Users.Features.Organization.Shared.Routes;

namespace Modules.Users.Features.Organization.CreateOrganization;

public class CreateOrganizationEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(RouteConsts.BaseRoute, Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] AddOrganizationRequest request,
        IValidator<AddOrganizationRequest> validator,
        ICreateOrganizationHandler handler,
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
