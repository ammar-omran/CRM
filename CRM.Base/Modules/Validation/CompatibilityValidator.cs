using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates that the module's version requirements are compatible with the current platform.
/// </summary>
public sealed class CompatibilityValidator : IModuleValidator
{
	private readonly string _currentPlatformVersion;
	private readonly string _currentSharedKernelVersion;

	/// <summary>
	/// Initializes the validator with current platform versions.
	/// </summary>
	/// <param name="currentPlatformVersion">The current platform version.</param>
	/// <param name="currentSharedKernelVersion">The current SharedKernel version.</param>
	public CompatibilityValidator(string currentPlatformVersion, string currentSharedKernelVersion)
	{
		_currentPlatformVersion = currentPlatformVersion;
		_currentSharedKernelVersion = currentSharedKernelVersion;
	}

	/// <inheritdoc />
	public string Name => "CompatibilityValidator";

	/// <inheritdoc />
	public void Validate(IModuleManifest manifest, ValidationReport report)
	{
		ValidatePlatformCompatibility(manifest, report);
		ValidateSharedKernelCompatibility(manifest, report);
		ValidateManifestSchemaCompatibility(manifest, report);
	}

	private void ValidatePlatformCompatibility(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Versioning.MinPlatformVersion))
		{
			return;
		}

		if (CompareVersions(_currentPlatformVersion, manifest.Versioning.MinPlatformVersion) < 0)
		{
			report.AddError("CompatibilityValidator",
				$"Platform version {_currentPlatformVersion} is below the minimum required " +
				$"{manifest.Versioning.MinPlatformVersion} by module '{manifest.Identity.ModuleId}'.",
				"PLATFORM_VERSION_TOO_LOW");
			return;
		}

		if (!string.IsNullOrWhiteSpace(manifest.Versioning.MaxPlatformVersion) &&
		    CompareVersions(_currentPlatformVersion, manifest.Versioning.MaxPlatformVersion) > 0)
		{
			report.AddError("CompatibilityValidator",
				$"Platform version {_currentPlatformVersion} exceeds the maximum supported " +
				$"{manifest.Versioning.MaxPlatformVersion} by module '{manifest.Identity.ModuleId}'.",
				"PLATFORM_VERSION_TOO_HIGH");
		}
	}

	private void ValidateSharedKernelCompatibility(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Versioning.MinSharedKernelVersion))
		{
			return;
		}

		if (CompareVersions(_currentSharedKernelVersion, manifest.Versioning.MinSharedKernelVersion) < 0)
		{
			report.AddError("CompatibilityValidator",
				$"SharedKernel version {_currentSharedKernelVersion} is below the minimum required " +
				$"{manifest.Versioning.MinSharedKernelVersion} by module '{manifest.Identity.ModuleId}'.",
				"SHARED_KERNEL_VERSION_TOO_LOW");
		}
	}

	private void ValidateManifestSchemaCompatibility(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Versioning.ManifestVersion))
		{
			return;
		}

		// V1 supports manifest schema version 1.x.x
		if (CompareVersions(manifest.Versioning.ManifestVersion, "2.0.0") >= 0)
		{
			report.AddWarning("CompatibilityValidator",
				$"Module '{manifest.Identity.ModuleId}' declares manifest schema version " +
				$"{manifest.Versioning.ManifestVersion}, which may not be fully supported by this platform.");
		}
	}

	/// <summary>
	/// Simple semantic version comparison. Returns negative if a &lt; b, zero if equal, positive if a &gt; b.
	/// </summary>
	private static int CompareVersions(string a, string b)
	{
		var partsA = a.Split('.');
		var partsB = b.Split('.');
		var length = Math.Max(partsA.Length, partsB.Length);

		for (var i = 0; i < length; i++)
		{
			var partA = i < partsA.Length && int.TryParse(partsA[i], out var valA) ? valA : 0;
			var partB = i < partsB.Length && int.TryParse(partsB[i], out var valB) ? valB : 0;

			var comparison = partA.CompareTo(partB);
			if (comparison != 0)
			{
				return comparison;
			}
		}

		return 0;
	}
}
