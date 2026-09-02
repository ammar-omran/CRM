using FluentValidation;

namespace Modules.Users.Features.Validators;

/// <summary>
/// FluentValidation validator enforcing the project's strong-password criteria:
/// at least 8 characters, with an uppercase letter, a lowercase letter, a digit,
/// and a special character.
/// </summary>
public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(password => password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[!@#$%^&*()\\-_=+\\[\\]{}|;:',.<>?/`~]").WithMessage("Password must contain at least one special character.");
    }
}
