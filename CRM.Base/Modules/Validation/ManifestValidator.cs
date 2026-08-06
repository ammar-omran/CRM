using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates that the manifest contains all required fields and follows the schema contract.
/// This is always the first validator in the pipeline.
/// </summary>
public sealed class ManifestValidator : IModuleValidator
{
	/// <inheritdoc />
	public string Name => "ManifestValidator";

	/// <inheritdoc />
	public void Validate(IModuleManifest manifest, ValidationReport report)
	{
		ValidateIdentity(manifest, report);
		ValidateVersioning(manifest, report);
		ValidateClassification(manifest, report);
		ValidateCapabilities(manifest, report);
		ValidateHealth(manifest, report);
		ValidateDatabase(manifest, report);

		if (manifest.Api is not null)
		{
			ValidateApi(manifest, report);
		}
	}

	private static void ValidateIdentity(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Identity.ModuleId))
		{
			report.AddError("ManifestValidator", "ModuleId is required.", "MISSING_MODULE_ID");
		}

		if (string.IsNullOrWhiteSpace(manifest.Identity.DisplayName))
		{
			report.AddError("ManifestValidator", "DisplayName is required.", "MISSING_DISPLAY_NAME");
		}

		if (manifest.Identity.ModuleId.Contains(' '))
		{
			report.AddError("ManifestValidator",
				$"ModuleId '{manifest.Identity.ModuleId}' must not contain spaces.",
				"INVALID_MODULE_ID");
		}
	}

	private static void ValidateVersioning(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Versioning.ManifestVersion))
		{
			report.AddError("ManifestValidator", "ManifestVersion is required.", "MISSING_MANIFEST_VERSION");
		}

		if (string.IsNullOrWhiteSpace(manifest.Versioning.ModuleVersion))
		{
			report.AddError("ManifestValidator", "ModuleVersion is required.", "MISSING_MODULE_VERSION");
		}

		if (string.IsNullOrWhiteSpace(manifest.Versioning.MinPlatformVersion))
		{
			report.AddError("ManifestValidator", "MinPlatformVersion is required.", "MISSING_MIN_PLATFORM_VERSION");
		}

		if (string.IsNullOrWhiteSpace(manifest.Versioning.MinSharedKernelVersion))
		{
			report.AddError("ManifestValidator",
				"MinSharedKernelVersion is required.",
				"MISSING_MIN_SHARED_KERNEL_VERSION");
		}
	}

	private static void ValidateClassification(IModuleManifest manifest, ValidationReport report)
	{
		var validKinds = Enum.GetNames<Modules.State.ModuleState>().Length > 0;
		// ModuleKind and ModuleAudience are enums — compiler validates at build time.
		// Runtime validation ensures they are defined values.
		if (!Enum.IsDefined(manifest.Classification.Kind))
		{
			report.AddError("ManifestValidator",
				$"Invalid ModuleKind value: {manifest.Classification.Kind}",
				"INVALID_MODULE_KIND");
		}

		if (!Enum.IsDefined(manifest.Classification.Audience))
		{
			report.AddError("ManifestValidator",
				$"Invalid ModuleAudience value: {manifest.Classification.Audience}",
				"INVALID_MODULE_AUDIENCE");
		}
	}

	private static void ValidateCapabilities(IModuleManifest manifest, ValidationReport report)
	{
		if (manifest.ProvidedCapabilities is null || manifest.ProvidedCapabilities.Count == 0)
		{
			report.AddError("ManifestValidator",
				"At least one capability must be provided.",
				"MISSING_CAPABILITIES");
			return;
		}

		var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var capability in manifest.ProvidedCapabilities)
		{
			if (string.IsNullOrWhiteSpace(capability.CapabilityId))
			{
				report.AddError("ManifestValidator",
					"CapabilityId is required for all provided capabilities.",
					"MISSING_CAPABILITY_ID");
				continue;
			}

			if (string.IsNullOrWhiteSpace(capability.DisplayName))
			{
				report.AddError("ManifestValidator",
					$"DisplayName is required for capability '{capability.CapabilityId}'.",
					"MISSING_CAPABILITY_DISPLAY_NAME");
			}

			if (!seenIds.Add(capability.CapabilityId))
			{
				report.AddError("ManifestValidator",
					$"Duplicate CapabilityId: '{capability.CapabilityId}'.",
					"DUPLICATE_CAPABILITY_ID");
			}
		}
	}

	private static void ValidateHealth(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Health.Endpoint))
		{
			report.AddError("ManifestValidator",
				"Health.Endpoint is required.",
				"MISSING_HEALTH_ENDPOINT");
		}
	}

	private static void ValidateDatabase(IModuleManifest manifest, ValidationReport report)
	{
		if (string.IsNullOrWhiteSpace(manifest.Database.SchemaName))
		{
			report.AddError("ManifestValidator",
				"Database.SchemaName is required.",
				"MISSING_SCHEMA_NAME");
		}
	}

	private static void ValidateApi(IModuleManifest manifest, ValidationReport report)
	{
		var api = manifest.Api!;

		foreach (var prefix in api.RoutePrefixs)
		{
			if (string.IsNullOrWhiteSpace(prefix))
			{
				report.AddError("ManifestValidator",
					"Api.RoutePrefix is required when Api is specified.",
					"MISSING_ROUTE_PREFIX");
			}
		}

		if (string.IsNullOrWhiteSpace(api.OpenApiEndpoint))
		{
			report.AddError("ManifestValidator",
				"Api.OpenApiEndpoint is required when Api is specified.",
				"MISSING_OPENAPI_ENDPOINT");
		}
	}
}
