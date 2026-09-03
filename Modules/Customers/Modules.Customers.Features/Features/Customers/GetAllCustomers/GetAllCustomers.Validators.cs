using FluentValidation;

namespace Modules.Customers.Features.Customers.GetAllCustomers;

public class GetAllCustomersRequestValidator : AbstractValidator<GetAllCustomersRequest>
{
    public GetAllCustomersRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");
    }
}
