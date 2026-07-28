using Modules.Customers.Features.DTOs;
using FluentValidation;


namespace Modules.Customers.Features.Validators
{
    public class CustomerLoginValidator : AbstractValidator<CustomerLoginDTO>
    {
        public CustomerLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(50).WithMessage("Password must be at most 50 characters.");
        }
    }
}
