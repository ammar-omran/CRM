namespace CRM.Base.Modules.Persistence;

/// <summary>
/// EF Core entity representing a registered module in the SQLite database.
/// Stores the manifest as JSON — the schema never changes when the manifest evolves.
/// </summary>
public sealed class RegisteredModuleEntity
{
	/// <summary>
	/// Auto-generated primary key.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// The module's unique identifier (e.g., "crm.users").
	/// </summary>
	public required string ModuleId { get; set; }

	/// <summary>
	/// Human-readable display name.
	/// </summary>
	public required string DisplayName { get; set; }

	/// <summary>
	/// The base URL where the module is hosted (e.g., "https://localhost:5003").
	/// </summary>
	public required string BaseUrl { get; set; }

	/// <summary>
	/// The full manifest serialized as JSON.
	/// This is the source of truth — the schema never changes when the manifest evolves.
	/// </summary>
	public required string ManifestJson { get; set; }

	/// <summary>
	/// Whether this module is enabled.
	/// </summary>
	public bool Enabled { get; set; } = true;

	/// <summary>
	/// UTC timestamp when this module was first registered.
	/// </summary>
	public DateTime RegisteredAt { get; set; }

	/// <summary>
	/// UTC timestamp when this module was last updated (e.g., manifest refresh).
	/// </summary>
	public DateTime UpdatedAt { get; set; }
}
