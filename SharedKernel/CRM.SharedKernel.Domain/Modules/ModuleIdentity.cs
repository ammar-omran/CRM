namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Uniquely identifies a module within the platform.
/// </summary>
public sealed record ModuleIdentity
{
	/// <summary>
	/// Globally unique, immutable identifier for this module.
	/// Naming convention: "crm.users", "crm.organizations", etc.
	/// Renaming a module requires a new ModuleId.
	/// </summary>
	public required string ModuleId { get; init; }

	/// <summary>
	/// Human-readable display name for this module.
	/// </summary>
	public required string DisplayName { get; init; }

	/// <summary>
	/// Optional description of what this module provides.
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// Optional author or team responsible for this module.
	/// </summary>
	public string? Author { get; init; }

	/// <summary>
	/// Optional tags for categorization and discovery.
	/// </summary>
	public IReadOnlyList<string>? Tags { get; init; }
}
