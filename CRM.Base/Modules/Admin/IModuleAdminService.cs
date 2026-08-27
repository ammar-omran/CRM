namespace CRM.Base.Modules.Admin;

/// <summary>
/// Thin application-level facade consumed by the Super Admin Razor Pages.
/// It does not implement platform logic — it orchestrates the existing
/// <see cref="CRM.Base.Modules.Persistence.ModuleCatalog"/>, the existing
/// <see cref="CRM.Base.Modules.Validation.ValidatorPipeline"/>, and the existing
/// persistence layer. All module lifecycle, validation, and gateway behaviour remains
/// owned by those existing components; the UI is purely a consumer.
/// </summary>
public interface IModuleAdminService
{
	/// <summary>Returns all registered modules as list summaries.</summary>
	IReadOnlyList<ModuleSummary> ListModules();

	/// <summary>Returns detailed manifest/registration information for a single module, or null.</summary>
	ModuleDetail? GetModule(string moduleId);

	/// <summary>Registers a module from its base URL using the existing registration pipeline.</summary>
	Task<ModuleOperationResult> RegisterModuleAsync(string url, CancellationToken cancellationToken = default);

	/// <summary>Enables a module using the existing enable pipeline.</summary>
	Task<ModuleOperationResult> EnableModuleAsync(string moduleId);

	/// <summary>Disables a module using the existing disable pipeline.</summary>
	Task<ModuleOperationResult> DisableModuleAsync(string moduleId);

	/// <summary>Removes a module from the platform using the existing removal pipeline.</summary>
	Task<ModuleOperationResult> RemoveModuleAsync(string moduleId);
}
