using FluentValidation;

namespace Modules.Users.Features.Users.RegisterUser;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone must be a valid phone number")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
