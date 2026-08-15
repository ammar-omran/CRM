using CRM.SharedKernel.Infrastructure.Policies;
using Microsoft.AspNetCore.Authorization;
using Modules.Ticketing.Domain.Policies;

namespace Modules.Ticketing.Infrastructure.Policies;

internal sealed class TicketingPolicyFactory : IPolicyFactory
{
	public string ModuleName => "Ticketing";

	public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
	{
		return PolicyFactoryExtensions.CreateClaimPoliciesFromConstants<TicketPolicyConstants>();
	}
}
