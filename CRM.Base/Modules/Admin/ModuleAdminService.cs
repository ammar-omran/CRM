using System.Text.Json;
using CRM.Base.Modules.Persistence;
using CRM.Base.Modules.State;
using CRM.Base.Modules.Validation;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.Extensions.Logging;

namespace CRM.Base.Modules.Admin;

/// <summary>
/// Default implementation of <see cref="IModuleAdminService"/>.
/// Delegates all platform behaviour to the existing <see cref="ModuleCatalog"/> and
/// <see cref="ValidatorPipeline"/>; it only adds the diagnostics needed by the UI to
/// surface real backend errors (e.g. unreachable module, duplicate registration, or
/// validation failures) rather than a generic message.
/// </summary>
public sealed class ModuleAdminService : IModuleAdminService
{
	private readonly ModuleCatalog _catalog;
	private readonly ValidatorPipeline _validatorPipeline;
	private readonly ILogger<ModuleAdminService> _logger;

	public ModuleAdminService(
		ModuleCatalog catalog,
		ValidatorPipeline validatorPipeline,
		ILogger<ModuleAdminService> logger)
	{
		_catalog = catalog;
		_validatorPipeline = validatorPipeline;
		_logger = logger;
	}

	/// <inheritdoc />
	public IReadOnlyList<ModuleSummary> ListModules() =>
		_catalog.GetAll()
			.Select(ToSummary)
			.ToList()
			.AsReadOnly();

	/// <inheritdoc />
	public ModuleDetail? GetModule(string moduleId)
	{
		var entry = _catalog.Get(moduleId);
		return entry is null ? null : ToDetail(entry);
	}

	/// <inheritdoc />
	public async Task<ModuleOperationResult> RegisterModuleAsync(string url, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(url);

		url = url.Trim().TrimEnd('/');

		if (string.IsNullOrWhiteSpace(url))
		{
			return ModuleOperationResult.Failure(null, "Module URL is required.");
		}

		// Enter the existing registration pipeline (fetch -> persist -> catalog -> gateway reload).
		var entry = await _catalog.RegisterAsync(url, cancellationToken);

		if (entry is not null)
		{
			_logger.LogInformation("Admin registered module '{ModuleId}' from {BaseUrl}", entry.Entity.ModuleId, url);
			return ModuleOperationResult.Success(
				entry.Entity.ModuleId,
				$"Module '{entry.Entity.DisplayName}' registered successfully.");
		}

		// Registration returned no entry — diagnose the real cause so the UI can show it.
		return await DiagnoseRegistrationFailureAsync(url, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<ModuleOperationResult> EnableModuleAsync(string moduleId)
	{
		if (string.IsNullOrWhiteSpace(moduleId))
		{
			return ModuleOperationResult.Failure(moduleId, "Module ID is required.");
		}

		var ok = await _catalog.EnableAsync(moduleId);
		return ok
			? ModuleOperationResult.Success(moduleId, $"Module '{moduleId}' enabled.")
			: ModuleOperationResult.Failure(moduleId, $"Module '{moduleId}' was not found.");
	}

	/// <inheritdoc />
	public async Task<ModuleOperationResult> DisableModuleAsync(string moduleId)
	{
		if (string.IsNullOrWhiteSpace(moduleId))
		{
			return ModuleOperationResult.Failure(moduleId, "Module ID is required.");
		}

		var ok = await _catalog.DisableAsync(moduleId);
		return ok
			? ModuleOperationResult.Success(moduleId, $"Module '{moduleId}' disabled.")
			: ModuleOperationResult.Failure(moduleId, $"Module '{moduleId}' was not found.");
	}

	/// <inheritdoc />
	public async Task<ModuleOperationResult> RemoveModuleAsync(string moduleId)
	{
		if (string.IsNullOrWhiteSpace(moduleId))
		{
			return ModuleOperationResult.Failure(moduleId, "Module ID is required.");
		}

		var ok = await _catalog.RemoveAsync(moduleId);
		return ok
			? ModuleOperationResult.Success(moduleId, $"Module '{moduleId}' removed from the platform.")
			: ModuleOperationResult.Failure(moduleId, $"Module '{moduleId}' was not found.");
	}

	private async Task<ModuleOperationResult> DiagnoseRegistrationFailureAsync(string url, CancellationToken cancellationToken)
	{
		IModuleManifest? manifest = null;

		try
		{
			manifest = await _catalog.FetchManifestAsync(url, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Admin failed to fetch manifest from {BaseUrl}", url);
		}

		if (manifest is null || manifest.Identity is null)
		{
			return ModuleOperationResult.Failure(
				null,
				$"Could not register module from '{url}'.",
				["The module URL is unreachable or did not return a valid manifest at /module/manifest."]);
		}

		var moduleId = manifest.Identity.ModuleId;

		if (_catalog.Get(moduleId) is not null)
		{
			return ModuleOperationResult.Failure(
				moduleId,
				$"Module '{moduleId}' is already registered.",
				["A module with this ID already exists in the platform."]);
		}

		var report = _validatorPipeline.Validate(manifest);

		if (!report.IsValid)
		{
			var errors = report.Errors
				.Select(e => string.IsNullOrWhiteSpace(e.Code) ? e.Message : $"[{e.Code}] {e.Message}")
				.ToList();

			return ModuleOperationResult.Failure(
				moduleId,
				$"Module '{moduleId}' failed validation.",
				errors);
		}

		// Reached only if the catalog rejected the entry for an unexpected reason.
		return ModuleOperationResult.Failure(
			moduleId,
			$"Module '{moduleId}' could not be registered.",
			["The registration pipeline returned no result. Check the platform logs for details."]);
	}

	private static ModuleSummary ToSummary(CatalogEntry entry) => new()
	{
		ModuleId = entry.Entity.ModuleId,
		DisplayName = entry.Entity.DisplayName,
		BaseUrl = entry.Entity.BaseUrl,
		Enabled = entry.Entity.Enabled,
		IsAvailable = entry.IsAvailable,
		State = entry.State,
		RegisteredAt = entry.Entity.RegisteredAt,
		Version = entry.Manifest?.Versioning.ModuleVersion,
		Kind = entry.Manifest?.Classification.Kind ?? ModuleKind.Domain,
		Audience = entry.Manifest?.Classification.Audience ?? ModuleAudience.Internal
	};

	private static ModuleDetail ToDetail(CatalogEntry entry) => new()
	{
		ModuleId = entry.Entity.ModuleId,
		DisplayName = entry.Entity.DisplayName,
		Description = entry.Manifest?.Identity.Description,
		Author = entry.Manifest?.Identity.Author,
		Tags = entry.Manifest?.Identity.Tags,
		Version = entry.Manifest?.Versioning.ModuleVersion ?? "—",
		Kind = entry.Manifest?.Classification.Kind ?? ModuleKind.Domain,
		Audience = entry.Manifest?.Classification.Audience ?? ModuleAudience.Internal,
		State = entry.State,
		Enabled = entry.Entity.Enabled,
		IsAvailable = entry.IsAvailable,
		BaseUrl = entry.Entity.BaseUrl,
		Api = entry.Manifest?.Api,
		Health = entry.Manifest?.Health,
		Database = entry.Manifest?.Database,
		ProvidedCapabilities = entry.Manifest?.ProvidedCapabilities ?? [],
		RequiredCapabilities = entry.Manifest?.RequiredCapabilities,
		ModuleDependencies = entry.Manifest?.ModuleDependencies,
		Policies = entry.Manifest?.Policies
	};
}
