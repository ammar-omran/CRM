using System.Linq;
using System.Reflection;

namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Helpers for turning *Constants classes (classes that hold one public <c>const string</c> field per policy)
/// into a module's declared <see cref="ModulePolicy"/> list. A module may pass several such classes — one per
/// entity — and they are aggregated into a single policy list.
/// </summary>
public static class PolicyConstants
{
	/// <summary>
	/// Builds the declared policies from one or more policy-constants classes. Every public <c>const string</c>
	/// field becomes a <see cref="ModulePolicy"/> whose <see cref="ModulePolicy.Name"/> is the constant value.
	/// </summary>
	/// <param name="constantsTypes">
	/// The constants classes to aggregate, e.g. <c>typeof(TicketPolicyConstants)</c>,
	/// <c>typeof(TicketCommentPolicyConstants)</c>, … One class per entity is the recommended shape.
	/// </param>
	public static IReadOnlyList<ModulePolicy> FromConstants(params Type[] constantsTypes)
	{
		ArgumentNullException.ThrowIfNull(constantsTypes);

		var policies = new List<ModulePolicy>();

		foreach (var type in constantsTypes)
		{
			if (type is null) continue;

			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
						 .Where(f => f.IsLiteral && f.FieldType == typeof(string)))
			{
				policies.Add(new ModulePolicy
				{
					Name = (string)field.GetRawConstantValue()!
				});
			}
		}

		return policies;
	}
}
