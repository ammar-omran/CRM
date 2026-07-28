namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents the health check endpoints exposed by a module.
/// The module decides what "healthy" means; the platform only needs to know where to check.
/// </summary>
public sealed record ModuleHealth
{
	/// <summary>
	/// Primary health check endpoint path (e.g., "/health").
	/// </summary>
	public required string Endpoint { get; init; }

	/// <summary>
	/// Liveness check endpoint path (e.g., "/alive").
	/// Returns 200 if the module process is running.
	/// </summary>
	public string? Liveness { get; init; }

	/// <summary>
	/// Readiness check endpoint path (optional).
	/// Returns 200 when the module is ready to serve traffic.
	/// Useful for modules with initialization or migration phases.
	/// </summary>
	public string? Readiness { get; init; }
}
