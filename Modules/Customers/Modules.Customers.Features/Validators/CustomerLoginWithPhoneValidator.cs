using Modules.Customers.Features.DTOs;
using FluentValidation;

namespace Modules.Customers.Features.Validators
{
    public class CustomerLoginWithPhoneValidator : AbstractValidator<CustomerLoginWithPhoneDTO>
    {
        public CustomerLoginWithPhoneValidator()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("This field is required: CountryCode.")
                .Matches(@"^\+\d{1,4}$").WithMessage("Country code must start with '+' followed by 1 to 4 digits.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("This field is required: PhoneNumber.")
                .Matches(@"^\d{10,15}$")
                .WithMessage("Phone number must contain only digits and be between 10 and 15 digits long.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("This field is required: Password.");
        }
    }
}
