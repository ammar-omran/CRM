using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates that declared API routes don't conflict with existing registered modules.
/// </summary>
public sealed class RouteValidator : IModuleValidator
{
	private readonly Func<IReadOnlyList<IModuleManifest>> _getRegisteredManifests;

	/// <summary>
	/// Initializes the validator with a function to retrieve all registered manifests.
	/// </summary>
	/// <param name="getRegisteredManifests">Function returning manifests of all registered modules.</param>
	public RouteValidator(Func<IReadOnlyList<IModuleManifest>> getRegisteredManifests)
	{
		_getRegisteredManifests = getRegisteredManifests;
	}

	/// <inheritdoc />
	public string Name => "RouteValidator";

	/// <inheritdoc />
	public void Validate(IModuleManifest manifest, ValidationReport report)
	{
		if (manifest.Api is null)
		{
			return;
		}

		var existingPrefixes = _getRegisteredManifests()
			.Where(m => m.Api is not null)
			.SelectMany(m => m.Api!.RoutePrefixs)
			.ToList();

		foreach (var prefix in manifest.Api.RoutePrefixs)
		{
			if (existingPrefixes.Contains(prefix, StringComparer.OrdinalIgnoreCase))
			{
				report.AddError("RouteValidator",
					$"Route prefix '{prefix}' is already claimed by another module.",
					"ROUTE_PREFIX_CONFLICT");
			}
		}

		// Validate anonymous paths are within the declared route prefix
		if (manifest.Api.AnonymousPaths is not null)
		{
			foreach (var path in manifest.Api.AnonymousPaths)
			{
				foreach (var prefix in manifest.Api.RoutePrefixs)
				{
					if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					{
						report.AddWarning("RouteValidator",
							$"Anonymous path '{path}' is outside the declared route prefix " +
							$"'{prefix}'. This may cause unexpected behavior.");
					}
				}
			}
		}
	}
}
