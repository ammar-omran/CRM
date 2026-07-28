using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Registration;

/// <summary>
/// Orchestrates the module registration pipeline.
/// Discovers, validates, initializes, and registers modules into the platform.
/// </summary>
public interface IModuleRegistrar
{
	/// <summary>
	/// Registers a single module through the full pipeline:
	/// Validate → Resolve Dependencies → Register → Initialize → Activate.
	/// </summary>
	/// <param name="manifest">The module manifest to register.</param>
	/// <returns>The registration result with diagnostics.</returns>
	Task<RegistrationResult> RegisterAsync(IModuleManifest manifest);

	/// <summary>
	/// Registers multiple modules in dependency-resolved order.
	/// </summary>
	/// <param name="manifests">The manifests to register.</param>
	/// <returns>The registration results for each module.</returns>
	Task<IReadOnlyList<RegistrationResult>> RegisterManyAsync(IReadOnlyList<IModuleManifest> manifests);

	/// <summary>
	/// Removes a module from the platform.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module was found and removed; false otherwise.</returns>
	Task<bool> RemoveAsync(string moduleId);

	/// <summary>
	/// Disables a module without removing it.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module was found and disabled; false otherwise.</returns>
	Task<bool> DisableAsync(string moduleId);

	/// <summary>
	/// Enables a previously disabled module.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module was found and enabled; false otherwise.</returns>
	Task<bool> EnableAsync(string moduleId);
}
