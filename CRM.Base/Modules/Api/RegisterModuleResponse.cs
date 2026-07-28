namespace CRM.Base.Modules.Api;

/// <summary>
/// Response body after registering a module.
/// </summary>
public sealed record RegisterModuleResponse
{
	/// <summary>
	/// Whether the registration succeeded.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// The module's unique identifier.
	/// </summary>
	public string? ModuleId { get; init; }

	/// <summary>
	/// The module's display name.
	/// </summary>
	public string? DisplayName { get; init; }

	/// <summary>
	/// Human-readable message describing the result.
	/// </summary>
	public string Message { get; init; } = string.Empty;

	/// <summary>
	/// Validation errors if registration failed.
	/// </summary>
	public List<string>? Errors { get; init; }
}

/// <summary>
/// A module item in list responses.
/// </summary>
public sealed record ModuleListItem
{
	/// <summary>
	/// The module's unique identifier.
	/// </summary>
	public required string ModuleId { get; init; }

	/// <summary>
	/// The module's display name.
	/// </summary>
	public required string DisplayName { get; init; }

	/// <summary>
	/// The base URL where the module is hosted.
	/// </summary>
	public required string BaseUrl { get; init; }

	/// <summary>
	/// Whether the module is enabled.
	/// </summary>
	public bool Enabled { get; init; }

	/// <summary>
	/// Whether the module is currently reachable.
	/// </summary>
	public bool IsAvailable { get; init; }

	/// <summary>
	/// The current runtime state.
	/// </summary>
	public string State { get; init; } = string.Empty;

	/// <summary>
	/// UTC timestamp when this module was registered.
	/// </summary>
	public DateTime RegisteredAt { get; init; }
}
