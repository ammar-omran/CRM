using FluentValidation;

namespace Modules.Customers.Features.Customers.ResendOtp;

public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.HashedEmail)
            .NotEmpty().WithMessage("HashedEmail is required");
    }
}
