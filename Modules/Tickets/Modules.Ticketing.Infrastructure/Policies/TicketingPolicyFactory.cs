using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Infrastructure.Policies;
using Microsoft.AspNetCore.Authorization;
using Modules.Ticketing.Domain.Policies;

namespace Modules.Ticketing.Infrastructure.Policies;

internal sealed class TicketingPolicyFactory : IPolicyFactory
{
	public string ModuleName => "Ticketing";

	public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
	{
		var policies = PolicyFactoryExtensions.CreateClaimPoliciesFromConstants<TicketPolicyConstants>();

		// OR-gate: holders of any ticket-view permission (full, mine, group — or itself)
		// satisfy ViewAnyPolicy. Used by single-ticket reads (details/history) and
		// create-form lookups so scoped roles aren't locked out; data-level scoping
		// is still enforced inside the handlers.
		policies[TicketPolicyConstants.ViewAnyPolicy] = p => p.RequireClaim(
			PermissionClaims.Type,
			TicketPolicyConstants.ViewPolicy,
			TicketPolicyConstants.ViewMinePolicy,
			TicketPolicyConstants.ViewGroupPolicy,
			TicketPolicyConstants.ViewAnyPolicy);

		return policies;
	}
}
