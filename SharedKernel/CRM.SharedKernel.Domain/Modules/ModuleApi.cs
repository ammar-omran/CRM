namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents the API contribution of a module.
/// Optional — non-HTTP modules do not need this section.
/// </summary>
public sealed record ModuleApi
{
	/// <summary>
	/// The module's namespace in the URL space (e.g., "/api/users").
	/// All routes declared in OpenAPI are relative to this prefix.
	/// </summary>
	public required string[] RoutePrefixs { get; init; }

	/// <summary>
	/// Path to the OpenAPI specification endpoint (e.g., "/swagger/v1/swagger.json").
	/// The platform fetches this at registration time to discover route details.
	/// </summary>
	public required string OpenApiEndpoint { get; init; }

	/// <summary>
	/// Paths that skip authentication at the gateway level.
	/// These are audited against the OpenAPI spec during registration.
	/// </summary>
	public IReadOnlyList<string>? AnonymousPaths { get; init; }
}
