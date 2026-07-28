namespace CRM.Base.Modules.Initialization;

/// <summary>
/// Contract for modules that perform initialization logic during registration.
/// The platform invokes this interface if the module implements it.
/// If the module does not implement it, the platform continues without error.
/// This keeps behavior code-driven rather than metadata-driven.
/// </summary>
public interface IModuleInitializer
{
	/// <summary>
	/// Called during module initialization. Perform any setup logic here
	/// (e.g., database migrations, seed data, cache warming).
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A task representing the asynchronous initialization.</returns>
	Task InitializeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Called during module shutdown. Perform any cleanup logic here.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A task representing the asynchronous shutdown.</returns>
	Task ShutdownAsync(CancellationToken cancellationToken = default);
}
