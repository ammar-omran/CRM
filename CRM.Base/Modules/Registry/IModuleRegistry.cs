using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Registry;

/// <summary>
/// Runtime storage for registered modules.
/// The registry owns the lifecycle state of all hosted modules.
/// </summary>
public interface IModuleRegistry
{
	/// <summary>
	/// Adds a module to the registry.
	/// </summary>
	/// <param name="module">The registered module to add.</param>
	/// <exception cref="InvalidOperationException">Thrown if a module with the same ID is already registered.</exception>
	void Register(RegisteredModule module);

	/// <summary>
	/// Removes a module from the registry.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module was found and removed; false otherwise.</returns>
	bool Remove(string moduleId);

	/// <summary>
	/// Gets a registered module by its unique identifier.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>The registered module, or null if not found.</returns>
	RegisteredModule? Get(string moduleId);

	/// <summary>
	/// Gets all registered modules.
	/// </summary>
	/// <returns>A read-only collection of all registered modules.</returns>
	IReadOnlyList<RegisteredModule> GetAll();

	/// <summary>
	/// Queries modules by their current state.
	/// </summary>
	/// <param name="state">The state to filter by.</param>
	/// <returns>Modules in the specified state.</returns>
	IReadOnlyList<RegisteredModule> GetByState(State.ModuleState state);

	/// <summary>
	/// Queries modules that provide a specific capability.
	/// </summary>
	/// <param name="capabilityId">The capability identifier to search for.</param>
	/// <returns>Modules providing the specified capability.</returns>
	IReadOnlyList<RegisteredModule> GetByCapability(string capabilityId);

	/// <summary>
	/// Checks whether a module with the given ID is registered.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if registered; false otherwise.</returns>
	bool Exists(string moduleId);

	/// <summary>
	/// Updates the state of a registered module.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <param name="state">The new state.</param>
	/// <returns>The updated module, or null if not found.</returns>
	RegisteredModule? UpdateState(string moduleId, State.ModuleState state);

	/// <summary>
	/// Marks a module as failed with the given reason.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <param name="reason">Human-readable failure reason.</param>
	/// <param name="details">Optional technical details.</param>
	/// <returns>The updated module, or null if not found.</returns>
	RegisteredModule? MarkFailed(string moduleId, string reason, string? details = null);
}
