using FluentValidation;
using Modules.Users.Features.Organizations.Shared.Requests;

namespace Modules.Users.Features.Organizations.GetOrganizationCustomers;

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
