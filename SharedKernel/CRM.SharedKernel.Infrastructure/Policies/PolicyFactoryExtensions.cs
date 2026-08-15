using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace CRM.SharedKernel.Infrastructure.Policies;

public static class PolicyFactoryExtensions
{
	public static Dictionary<string, Action<AuthorizationPolicyBuilder>> CreateClaimPoliciesFromConstants<T>() =>
		typeof(T)
			.GetFields(BindingFlags.Public | BindingFlags.Static)
			.Where(f => f.IsLiteral && f.FieldType == typeof(string))
			.ToDictionary(
				f => (string)f.GetRawConstantValue()!,
				f => new Action<AuthorizationPolicyBuilder>(p => p.RequireClaim((string)f.GetRawConstantValue()!)));
}
