using System.Diagnostics;
using CRM.Base.Modules.DependencyResolution;
using CRM.Base.Modules.Events;
using CRM.Base.Modules.Initialization;
using CRM.Base.Modules.Registry;
using CRM.Base.Modules.State;
using CRM.Base.Modules.Validation;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Base.Modules.Registration;

/// <summary>
/// Orchestrates the module registration pipeline.
/// Composes validators, dependency resolution, and initialization into a single cohesive flow.
/// Each stage has a single responsibility; the registrar only coordinates.
/// </summary>
public sealed class ModuleRegistrar : IModuleRegistrar
{
	private readonly IModuleRegistry _registry;
	private readonly ValidatorPipeline _validatorPipeline;
	private readonly IDependencyResolver _dependencyResolver;
	private readonly IServiceProvider _serviceProvider;
	private readonly IEventPublisher _eventPublisher;
	private readonly ILogger<ModuleRegistrar> _logger;

	/// <summary>
	/// Initializes the registrar with all required dependencies.
	/// </summary>
	public ModuleRegistrar(
		IModuleRegistry registry,
		ValidatorPipeline validatorPipeline,
		IDependencyResolver dependencyResolver,
		IServiceProvider serviceProvider,
		IEventPublisher eventPublisher,
	 ILogger<ModuleRegistrar> logger)
	{
		_registry = registry;
		_validatorPipeline = validatorPipeline;
		_dependencyResolver = dependencyResolver;
		_serviceProvider = serviceProvider;
		_eventPublisher = eventPublisher;
		_logger = logger;
	}

	/// <inheritdoc />
	public async Task<RegistrationResult> RegisterAsync(IModuleManifest manifest)
	{
		ArgumentNullException.ThrowIfNull(manifest);

		var stopwatch = Stopwatch.StartNew();
		var moduleId = manifest.Identity.ModuleId;

		_logger.LogInformation("Starting registration pipeline for module '{ModuleId}'", moduleId);

		try
		{
			// Stage 1: Discover (already done — manifest was provided)
			await _eventPublisher.PublishAsync(
				new ModuleDiscoveredEvent(moduleId, manifest.Versioning.ModuleVersion),
				CancellationToken.None);

			// Stage 2: Validate
			var validationReport = _validatorPipeline.Validate(manifest);

			if (!validationReport.IsValid)
			{
				stopwatch.Stop();

				var errorSummary = string.Join("; ",
					validationReport.Errors.Select(e => $"[{e.Code}] {e.Message}"));

				_logger.LogError("Module '{ModuleId}' failed validation: {Errors}",
					moduleId, errorSummary);

				await PublishFailure(moduleId, "Validation", errorSummary);

				return RegistrationResult.Failure(
					moduleId,
					"Validation",
					errorSummary,
					validationReport,
					stopwatch.Elapsed);
			}

			await _eventPublisher.PublishAsync(
				new ModuleValidatedEvent(moduleId, manifest.Versioning.ModuleVersion),
				CancellationToken.None);

			// Stage 3: Resolve Dependencies
			var orderedManifests = _dependencyResolver.ResolveStartupOrder([manifest]);
			// For single module registration, the resolved order should be just this module.
			// Multi-module registration handles full graph resolution.

			// Stage 4: Register
			if (_registry.Exists(moduleId))
			{
				stopwatch.Stop();
				return RegistrationResult.Failure(
					moduleId,
					"Register",
					$"Module '{moduleId}' is already registered.",
					validationReport,
					stopwatch.Elapsed);
			}

			var registeredModule = new RegisteredModule
			{
				Manifest = manifest,
				ValidationReport = validationReport
			};

			_registry.Register(registeredModule);
			_registry.UpdateState(moduleId, ModuleState.Registered);

			await _eventPublisher.PublishAsync(
				new ModuleRegisteredEvent(moduleId, manifest.Versioning.ModuleVersion, DateTime.UtcNow),
				CancellationToken.None);

			// Stage 5: Initialize
			var initializer = _serviceProvider.GetService<IModuleInitializer>();

			if (initializer is not null)
			{
				_registry.UpdateState(moduleId, ModuleState.Initializing);

				await _eventPublisher.PublishAsync(
					new ModuleInitializingEvent(moduleId),
					CancellationToken.None);

				try
				{
					await initializer.InitializeAsync();
				}
				catch (Exception ex)
				{
					stopwatch.Stop();
					_registry.MarkFailed(moduleId, "Initialization failed", ex.Message);

					_logger.LogError(ex, "Module '{ModuleId}' initialization failed", moduleId);

					await PublishFailure(moduleId, "Initialization", ex.Message);

					return RegistrationResult.Failure(
						moduleId,
						"Initialization",
						ex.Message,
						validationReport,
						stopwatch.Elapsed);
				}
			}

			// Stage 6: Activate
			_registry.UpdateState(moduleId, ModuleState.Running);

			stopwatch.Stop();

			await _eventPublisher.PublishAsync(
				new ModuleStartedEvent(moduleId, DateTime.UtcNow),
				CancellationToken.None);

			_logger.LogInformation(
				"Module '{ModuleId}' registered successfully in {Elapsed}",
				moduleId, stopwatch.Elapsed);

			return RegistrationResult.Success(registeredModule, validationReport, stopwatch.Elapsed);
		}
		catch (Exception ex) when (ex is not (DependencyCycleException or MissingDependencyException))
		{
			stopwatch.Stop();

			_registry.MarkFailed(moduleId, "Unexpected error", ex.Message);

			_logger.LogError(ex, "Module '{ModuleId}' registration failed unexpectedly", moduleId);

			await PublishFailure(moduleId, "Unexpected", ex.Message);

			return RegistrationResult.Failure(
				moduleId,
				"Unexpected",
				ex.Message,
				duration: stopwatch.Elapsed);
		}
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<RegistrationResult>> RegisterManyAsync(IReadOnlyList<IModuleManifest> manifests)
	{
		ArgumentNullException.ThrowIfNull(manifests);

		_logger.LogInformation("Resolving startup order for {Count} modules", manifests.Count);

		IReadOnlyList<IModuleManifest> ordered;

		try
		{
			ordered = _dependencyResolver.ResolveStartupOrder(manifests);
		}
		catch (DependencyCycleException ex)
		{
			_logger.LogError("Dependency cycle detected: {Message}", ex.Message);
			return
			[
				RegistrationResult.Failure("batch", "DependencyResolution", ex.Message)
			];
		}
		catch (MissingDependencyException ex)
		{
			_logger.LogError("Missing dependency: {Message}", ex.Message);
			return
			[
				RegistrationResult.Failure("batch", "DependencyResolution", ex.Message)
			];
		}

		var results = new List<RegistrationResult>();

		foreach (var manifest in ordered)
		{
			var result = await RegisterAsync(manifest);
			results.Add(result);

			// Fail fast — stop processing if a required module fails
			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Stopping batch registration after module '{ModuleId}' failed",
					manifest.Identity.ModuleId);
				break;
			}
		}

		return results.AsReadOnly();
	}

	/// <inheritdoc />
	public async Task<bool> RemoveAsync(string moduleId)
	{
		var module = _registry.Get(moduleId);
		if (module is null)
		{
			return false;
		}

		// Shutdown if running
		if (module.State == ModuleState.Running)
		{
			var initializer = _serviceProvider.GetService<IModuleInitializer>();
			if (initializer is not null)
			{
				await initializer.ShutdownAsync();
			}
		}

		_registry.UpdateState(moduleId, ModuleState.Removed);
		_registry.Remove(moduleId);

		await _eventPublisher.PublishAsync(
			new ModuleRemovedEvent(moduleId, DateTime.UtcNow),
			CancellationToken.None);

		_logger.LogInformation("Module '{ModuleId}' removed", moduleId);

		return true;
	}

	/// <inheritdoc />
	public Task<bool> DisableAsync(string moduleId)
	{
		var module = _registry.UpdateState(moduleId, ModuleState.Disabled);
		if (module is null)
		{
			return Task.FromResult(false);
		}

		_logger.LogInformation("Module '{ModuleId}' disabled", moduleId);
		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public Task<bool> EnableAsync(string moduleId)
	{
		var module = _registry.Get(moduleId);
		if (module is null || module.State != ModuleState.Disabled)
		{
			return Task.FromResult(false);
		}

		_registry.UpdateState(moduleId, ModuleState.Running);

		_logger.LogInformation("Module '{ModuleId}' enabled", moduleId);
		return Task.FromResult(true);
	}

	private async Task PublishFailure(string moduleId, string stage, string details)
	{
		await _eventPublisher.PublishAsync(
			new ModuleRegistrationFailedEvent(moduleId, stage, details),
			CancellationToken.None);
	}
}
