namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents an authorization policy defined by a module.
/// Policy names are globally unique and prefixed with the module name (e.g., "Users.Read").
/// </summary>
public sealed record ModulePolicy
{
	/// <summary>
	/// Globally unique policy name, prefixed with the module name.
	/// Examples: "Users.Read", "Organizations.Create", "Tickets.Close".
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Optional human-readable description of what this policy authorizes.
	/// </summary>
	public string? Description { get; init; }
}
