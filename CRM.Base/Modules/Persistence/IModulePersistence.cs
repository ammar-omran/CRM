namespace CRM.Base.Modules.Persistence;

/// <summary>
/// Handles SQLite persistence for registered modules.
/// The platform owns this storage — modules never interact with it directly.
/// </summary>
public interface IModulePersistence
{
	/// <summary>
	/// Saves a new module to the database.
	/// </summary>
	/// <param name="entity">The module entity to save.</param>
	Task SaveAsync(RegisteredModuleEntity entity);

	/// <summary>
	/// Updates an existing module in the database.
	/// </summary>
	/// <param name="entity">The module entity to update.</param>
	Task UpdateAsync(RegisteredModuleEntity entity);

	/// <summary>
	/// Removes a module from the database by its ModuleId.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module was found and removed; false otherwise.</returns>
	Task<bool> RemoveAsync(string moduleId);

	/// <summary>
	/// Loads all registered modules from the database.
	/// </summary>
	/// <returns>All registered module entities.</returns>
	Task<IReadOnlyList<RegisteredModuleEntity>> LoadAllAsync();

	/// <summary>
	/// Checks whether a module with the given ID exists in the database.
	/// </summary>
	/// <param name="moduleId">The unique module identifier.</param>
	/// <returns>True if the module exists; false otherwise.</returns>
	Task<bool> ExistsAsync(string moduleId);
}
