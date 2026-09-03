using FluentValidation;
using Modules.Users.Features.Agents.Shared.Requests;

namespace Modules.Users.Features.Agents.CreateAgent;

public class CreateAgentRequestValidator : AbstractValidator<AddAgentRequest>
{
    public CreateAgentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MaximumLength(450).WithMessage("UserId must be at most 450 characters");
    }
}
