namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Version information for the manifest schema and the module itself.
/// Two separate version tracks enable independent evolution of the contract and the implementation.
/// </summary>
public sealed record ModuleVersioning
{
	/// <summary>
	/// The manifest schema version this module implements (e.g., "1.0.0").
	/// The platform uses this to validate compatibility.
	/// </summary>
	public required string ManifestVersion { get; init; }

	/// <summary>
	/// The module's own version (e.g., "1.2.0").
	/// Evolves independently of the manifest schema version.
	/// </summary>
	public required string ModuleVersion { get; init; }

	/// <summary>
	/// Minimum platform version required by this module.
	/// </summary>
	public required string MinPlatformVersion { get; init; }

	/// <summary>
	/// Maximum platform version supported by this module. Null means no upper bound.
	/// </summary>
	public string? MaxPlatformVersion { get; init; }

	/// <summary>
	/// Minimum SharedKernel version required by this module.
	/// </summary>
	public required string MinSharedKernelVersion { get; init; }
}
