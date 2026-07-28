namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents a dependency on another module.
/// The platform uses this to resolve startup order and validate compatibility.
/// </summary>
public sealed record ModuleDependency
{
	/// <summary>
	/// The ModuleId of the required module.
	/// </summary>
	public required string ModuleId { get; init; }

	/// <summary>
	/// Minimum version of the required module. Null means any version.
	/// </summary>
	public string? MinVersion { get; init; }

	/// <summary>
	/// Maximum version of the required module. Null means any version.
	/// </summary>
	public string? MaxVersion { get; init; }

	/// <summary>
	/// Whether this dependency is required for the module to function.
	/// Soft dependencies allow the module to start in a degraded mode.
	/// Defaults to true.
	/// </summary>
	public bool IsRequired { get; init; } = true;

	/// <summary>
	/// Optional documentation explaining why this dependency exists.
	/// Displayed in the admin UI dependency graph.
	/// </summary>
	public string? Purpose { get; init; }
}
