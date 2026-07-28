namespace CRM.Base.Modules.Api;

/// <summary>
/// Request body for registering a new module.
/// </summary>
public sealed record RegisterModuleRequest
{
	/// <summary>
	/// The base URL of the module to register (e.g., "https://localhost:5003").
	/// </summary>
	public required string Url { get; init; }
}
