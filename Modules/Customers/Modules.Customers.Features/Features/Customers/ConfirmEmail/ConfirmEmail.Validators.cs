using FluentValidation;

namespace Modules.Customers.Features.Customers.ConfirmEmail;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.HashedEmail)
            .NotEmpty().WithMessage("HashedEmail is required");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("Otp is required")
            .Length(6).WithMessage("Otp must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Otp must contain only digits");
    }
}
