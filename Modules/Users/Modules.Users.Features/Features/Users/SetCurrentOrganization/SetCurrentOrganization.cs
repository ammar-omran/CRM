using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Authentication;
using Modules.Users.Features.Users.Shared.Routes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Modules.Users.Features.Users.SetCurrentOrganization;

public sealed record SetCurrentOrganizationRequest(int OrganizationId);



internal interface ISetCurrentOrganizationHandler : IHandler
{
	Task<Result<Success>> HandleAsync(string userId, SetCurrentOrganizationRequest request, CancellationToken cancellationToken);
}

internal sealed class SetCurrentOrganizationHandler(IClientAuthorizationService authorizationService)
		: ISetCurrentOrganizationHandler
{
	public Task<Result<Success>> HandleAsync(string userId, SetCurrentOrganizationRequest request, CancellationToken cancellationToken)
	{
		return authorizationService.SetCurrentOrganizationAsync(userId, request.OrganizationId, cancellationToken);
	}
}

internal class SetCurrentOrganizationEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost(RouteConsts.SetCurrentOrganization, Handle)
				.RequireAuthorization();
	}

	private static async Task<Microsoft.AspNetCore.Http.IResult> Handle(
			[FromBody] SetCurrentOrganizationRequest request,
			HttpContext httpContext,
			IValidator<SetCurrentOrganizationRequest> validator,
			ISetCurrentOrganizationHandler handler,
			CancellationToken cancellationToken)
	{
		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
							?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
							?? httpContext.User.FindFirst("userid")?.Value
							?? httpContext.User.FindFirst("sub")?.Value;

		if (string.IsNullOrWhiteSpace(userId))
		{
			return Results.Unauthorized();
		}

		var response = await handler.HandleAsync(userId, request, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}

internal class SetCurrentOrganizationRequestValidator : AbstractValidator<SetCurrentOrganizationRequest>
{
	public SetCurrentOrganizationRequestValidator()
	{
		RuleFor(x => x.OrganizationId)
				.GreaterThan(0).WithMessage("OrganizationId must be greater than 0");
	}
}
