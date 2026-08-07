using FluentValidation;
using Modules.Users.Features.Agent.Shared.Requests;

namespace Modules.Users.Features.Agent.GetAgents;

public class GetAgentsRequestValidator : AbstractValidator<AgentByRequest>
{
    public GetAgentsRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be non-negative");
    }
}
