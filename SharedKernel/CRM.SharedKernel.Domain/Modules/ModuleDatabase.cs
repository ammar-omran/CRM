namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents the data ownership of a module.
/// The module owns its schema; no other module may reference tables outside its own schema.
/// </summary>
public sealed record ModuleDatabase
{
	/// <summary>
	/// The database schema name owned by this module (e.g., "users", "organizations").
	/// </summary>
	public required string SchemaName { get; init; }

	/// <summary>
	/// Optional name of the connection string in platform configuration.
	/// If null, the platform uses the default connection.
	/// </summary>
	public string? ConnectionName { get; init; }
}
