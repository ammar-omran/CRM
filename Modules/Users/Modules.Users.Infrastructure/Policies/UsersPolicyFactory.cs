using Microsoft.AspNetCore.Authorization;
using CRM.SharedKernel.Infrastructure.Policies;
using Modules.Users.Domain.Policies;

namespace Modules.Users.Infrastructure.Policies;

internal sealed class UsersPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Users";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
			return PolicyFactoryExtensions.CreateClaimPoliciesFromConstants<UserPolicyConstants>();
    }
}
