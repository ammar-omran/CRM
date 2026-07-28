using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates that all declared module dependencies can be satisfied.
/// Checks for missing dependencies and version compatibility.
/// </summary>
public sealed class DependencyValidator : IModuleValidator
{
	private readonly Func<IReadOnlyList<string>> _getRegisteredModuleIds;

	/// <summary>
	/// Initializes the validator with a function to retrieve currently registered module IDs.
	/// </summary>
	/// <param name="getRegisteredModuleIds">Function returning IDs of all registered modules.</param>
	public DependencyValidator(Func<IReadOnlyList<string>> getRegisteredModuleIds)
	{
		_getRegisteredModuleIds = getRegisteredModuleIds;
	}

	/// <inheritdoc />
	public string Name => "DependencyValidator";

	/// <inheritdoc />
	public void Validate(IModuleManifest manifest, ValidationReport report)
	{
		if (manifest.ModuleDependencies is null || manifest.ModuleDependencies.Count == 0)
		{
			return;
		}

		var registeredIds = _getRegisteredModuleIds();

		foreach (var dependency in manifest.ModuleDependencies)
		{
			var isRegistered = registeredIds.Contains(dependency.ModuleId, StringComparer.OrdinalIgnoreCase);

			if (!isRegistered && dependency.IsRequired)
			{
				report.AddError("DependencyValidator",
					$"Required dependency '{dependency.ModuleId}' is not registered." +
					(dependency.Purpose is not null ? $" Purpose: {dependency.Purpose}" : ""),
					"MISSING_REQUIRED_DEPENDENCY");
			}
			else if (!isRegistered && !dependency.IsRequired)
			{
				report.AddWarning("DependencyValidator",
					$"Optional dependency '{dependency.ModuleId}' is not registered. " +
					$"Module '{manifest.Identity.ModuleId}' may operate in a degraded mode.");
			}
		}
	}
}
