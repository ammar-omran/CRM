using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates that capability IDs are unique across all registered modules.
/// Prevents capability collisions in the platform.
/// </summary>
public sealed class CapabilityValidator : IModuleValidator
{
	private readonly Func<IReadOnlyList<IModuleManifest>> _getRegisteredManifests;

	/// <summary>
	/// Initializes the validator with a function to retrieve all registered manifests.
	/// </summary>
	/// <param name="getRegisteredManifests">Function returning manifests of all registered modules.</param>
	public CapabilityValidator(Func<IReadOnlyList<IModuleManifest>> getRegisteredManifests)
	{
		_getRegisteredManifests = getRegisteredManifests;
	}

	/// <inheritdoc />
	public string Name => "CapabilityValidator";

	/// <inheritdoc />
	public void Validate(IModuleManifest manifest, ValidationReport report)
	{
		if (manifest.ProvidedCapabilities is null || manifest.ProvidedCapabilities.Count == 0)
		{
			return;
		}

		var existingCapabilities = _getRegisteredManifests()
			.SelectMany(m => m.ProvidedCapabilities)
			.ToDictionary(
				c => c.CapabilityId,
				c => c,
				StringComparer.OrdinalIgnoreCase);

		foreach (var capability in manifest.ProvidedCapabilities)
		{
			if (existingCapabilities.TryGetValue(capability.CapabilityId, out var existing))
			{
				report.AddError("CapabilityValidator",
					$"Capability '{capability.CapabilityId}' is already provided by module " +
					$"'{existing.DisplayName}' (version {existing.Version}). " +
					"Each capability must have a single owner.",
					"CAPABILITY_CONFLICT");
			}
		}
	}
}
