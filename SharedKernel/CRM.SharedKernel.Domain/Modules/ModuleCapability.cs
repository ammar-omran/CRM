namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents a unit of business functionality that a module provides to the platform.
/// Capabilities are the primary abstraction the platform discovers and reasons about.
/// </summary>
public sealed record ModuleCapability
{
	/// <summary>
	/// Unique identifier for this capability within the module.
	/// The combination of ModuleId + CapabilityId is globally unique.
	/// </summary>
	public required string CapabilityId { get; init; }

	/// <summary>
	/// Human-readable display name for this capability.
	/// </summary>
	public required string DisplayName { get; init; }

	/// <summary>
	/// Optional description of what this capability provides.
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// Free-form category string for capability-based discovery.
	/// Examples: "Identity", "Ticketing", "Organization".
	/// </summary>
	public string? Category { get; init; }

	/// <summary>
	/// Semantic version of this capability. Defaults to "1.0.0".
	/// </summary>
	public string Version { get; init; } = "1.0.0";

	/// <summary>
	/// Whether this capability is required by the module.
	/// Required capabilities cannot be disabled by the platform.
	/// Defaults to true.
	/// </summary>
	public bool IsRequired { get; init; } = true;
}
