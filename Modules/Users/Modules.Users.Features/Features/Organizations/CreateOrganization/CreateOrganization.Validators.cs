using FluentValidation;
using Modules.Users.Features.Organizations.Shared.Requests;

namespace Modules.Users.Features.Organizations.CreateOrganization;

public class CreateOrganizationRequestValidator : AbstractValidator<AddOrganizationRequest>
{
	public CreateOrganizationRequestValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Organization name is required")
			.MaximumLength(128);

		// RuleFor(x => x.Agents)
		//     .NotEmpty().WithMessage("At least one agent is required");

		RuleForEach(x => x.Agents)
			.SetValidator(new OrganizationAgentValidator());

		// RuleFor(x => x.Customers)
		//     .NotEmpty().WithMessage("At least one customer is required");

		RuleForEach(x => x.Customers)
			.SetValidator(new OrganizationCustomerValidator());
	}
}

public class OrganizationAgentValidator : AbstractValidator<AddOrganizationRequestAgents>
{
	public OrganizationAgentValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0).WithMessage("AgentId must be valid");

		RuleFor(x => x.Role)
			.NotEmpty().WithMessage("Agent role must be valid");
	}
}

public class OrganizationCustomerValidator : AbstractValidator<AddOrganizationCustomerRequest>
{
	public OrganizationCustomerValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0).WithMessage("CustomerId must be valid");
	}
}
