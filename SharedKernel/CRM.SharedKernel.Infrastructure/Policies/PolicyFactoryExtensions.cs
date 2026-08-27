using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using CRM.SharedKernel.Domain.Authorization;

namespace CRM.SharedKernel.Infrastructure.Policies;

public static class PolicyFactoryExtensions
{
    public static Dictionary<string, Action<AuthorizationPolicyBuilder>> CreateClaimPoliciesFromConstants<T>() =>
        typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .ToDictionary(
                f => (string)f.GetRawConstantValue()!,
                f => new Action<AuthorizationPolicyBuilder>(p => p.RequireClaim(PermissionClaims.Type, (string)f.GetRawConstantValue()!)));
}
