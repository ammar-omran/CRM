using FluentValidation;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Authentication;

namespace Modules.Users.Features.Organizations.SetCurrentOrganization;

public sealed record SetCurrentOrganizationRequest(int OrganizationId);



public interface ISetCurrentOrganizationHandler : IHandler
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

public sealed class SetCurrentOrganizationRequestValidator : AbstractValidator<SetCurrentOrganizationRequest>
{
	public SetCurrentOrganizationRequestValidator()
	{
		RuleFor(x => x.OrganizationId)
				.GreaterThan(0).WithMessage("OrganizationId must be greater than 0");
	}
}
