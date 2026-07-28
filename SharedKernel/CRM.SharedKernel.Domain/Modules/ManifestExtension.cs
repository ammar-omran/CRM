using System.Text.Json;

namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents a forward-compatible extension point in the manifest.
/// The platform ignores extensions it doesn't recognize (fail-open for forward compatibility).
/// </summary>
public sealed record ManifestExtension
{
	/// <summary>
	/// Unique identifier for this extension point.
	/// </summary>
	public required string ExtensionPoint { get; init; }

	/// <summary>
	/// The manifest schema version that introduced this extension.
	/// </summary>
	public required string ManifestVersion { get; init; }

	/// <summary>
	/// Extension-specific data as a JSON element.
	/// Using JsonElement ensures reliable serialization across platform versions.
	/// </summary>
	public required JsonElement Data { get; init; }

	/// <summary>
	/// Optional human-readable description of this extension.
	/// </summary>
	public string? Description { get; init; }
}
