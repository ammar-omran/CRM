using FluentValidation;
using Modules.Organizations.Features.Organization.Shared.Requests;

namespace Modules.Organizations.Features.Organization.GetOrganizationCustomers;

public class GetOrganizationCustomersRequestValidator : AbstractValidator<GetOrganizationCustomersRequest>
{
    public GetOrganizationCustomersRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be non-negative");
    }
}
