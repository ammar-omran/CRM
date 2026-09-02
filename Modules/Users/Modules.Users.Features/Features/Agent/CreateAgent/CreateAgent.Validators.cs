using FluentValidation;
using Modules.Users.Features.Agent.Shared.Requests;

namespace Modules.Users.Features.Agent.CreateAgent;

public class CreateAgentRequestValidator : AbstractValidator<AddAgentRequest>
{
    public CreateAgentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MaximumLength(450).WithMessage("UserId must be at most 450 characters");
    }
}
