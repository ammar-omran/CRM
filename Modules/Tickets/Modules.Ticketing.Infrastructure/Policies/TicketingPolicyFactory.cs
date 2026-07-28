using CRM.SharedKernel.Infrastructure.Policies;
using Microsoft.AspNetCore.Authorization;
using Modules.Ticketing.Domain.Policies;

namespace Modules.Ticketing.Infrastructure.Policies;

internal sealed class TicketingPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Ticketing";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            [TicketPolicyConstants.ViewPolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.ViewPolicy),
            [TicketPolicyConstants.CreatePolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.CreatePolicy),
            [TicketPolicyConstants.UpdatePolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.UpdatePolicy),
            [TicketPolicyConstants.DeletePolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.DeletePolicy),
            [TicketPolicyConstants.AssignPolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.AssignPolicy),
            [TicketPolicyConstants.ReplyPolicy] = policy =>
                policy.RequireClaim(TicketPolicyConstants.ReplyPolicy)
        };
    }
}
