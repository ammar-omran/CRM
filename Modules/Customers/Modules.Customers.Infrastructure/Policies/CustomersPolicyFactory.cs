using CRM.SharedKernel.Infrastructure.Policies;
using Microsoft.AspNetCore.Authorization;
using Modules.Customers.Domain.Policies;

namespace Modules.Customers.Infrastructure.Policies;

internal sealed class CustomersPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Customers";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            [CustomerPolicyConstants.ViewPolicy] = policy =>
                policy.RequireClaim(CustomerPolicyConstants.ViewPolicy),
            [CustomerPolicyConstants.CreatePolicy] = policy =>
                policy.RequireClaim(CustomerPolicyConstants.CreatePolicy),
            [CustomerPolicyConstants.UpdatePolicy] = policy =>
                policy.RequireClaim(CustomerPolicyConstants.UpdatePolicy),
            [CustomerPolicyConstants.DeletePolicy] = policy =>
                policy.RequireClaim(CustomerPolicyConstants.DeletePolicy)
        };
    }
}
