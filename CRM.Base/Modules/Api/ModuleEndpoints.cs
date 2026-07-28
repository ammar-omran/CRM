using CRM.Base.Modules.Persistence;
using CRM.Base.Modules.Validation;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Base.Modules.Api;

/// <summary>
/// Minimal API endpoints for module registration.
/// Provides POST /platform/modules and GET /platform/modules.
/// </summary>
public static class ModuleEndpoints
{
	/// <summary>
	/// Maps the module registration endpoints.
	/// </summary>
	/// <param name="app">The web application.</param>
	/// <returns>The web application for chaining.</returns>
	public static WebApplication MapModuleEndpoints(this WebApplication app)
	{
		app.MapPost("/platform/modules", HandleRegisterModule);
		app.MapGet("/platform/modules", HandleGetModules);
		app.MapGet("/platform/modules/{moduleId}", HandleGetModule);
		app.MapDelete("/platform/modules/{moduleId}", HandleRemoveModule);

		return app;
	}

	private static async Task<IResult> HandleRegisterModule(
		RegisterModuleRequest request,
		ModuleCatalog catalog,
		ValidatorPipeline validatorPipeline,
		ILoggerFactory loggerFactory)
	{
		var logger = loggerFactory.CreateLogger("ModuleRegistration");

		if (string.IsNullOrWhiteSpace(request.Url))
		{
			return Results.BadRequest(new RegisterModuleResponse
			{
				Success = false,
				Message = "URL is required.",
				Errors = ["Url field must not be empty."]
			});
		}

		// Register (fetches manifest, validates, persists, adds to catalog)
		var entry = await catalog.RegisterAsync(request.Url);

		if (entry is null)
		{
			return Results.BadRequest(new RegisterModuleResponse
			{
				Success = false,
				Message = $"Failed to register module from '{request.Url}'. " +
				          "The URL may be unreachable or the manifest is invalid.",
				Errors = [$"Could not fetch or deserialize manifest from {request.Url}/module/manifest"]
			});
		}

		if (entry.Manifest is null)
		{
			return Results.BadRequest(new RegisterModuleResponse
			{
				Success = false,
				ModuleId = entry.Entity.ModuleId,
				Message = $"Module '{entry.Entity.ModuleId}' registered but manifest could not be deserialized.",
				Errors = ["Manifest JSON is invalid."]
			});
		}

		// Run validation pipeline
		var validationReport = validatorPipeline.Validate(entry.Manifest);

		if (!validationReport.IsValid)
		{
			var errors = validationReport.Errors
				.Select(e => $"[{e.Code}] {e.Message}")
				.ToList();

			logger.LogWarning(
				"Module '{ModuleId}' failed validation: {Errors}",
				entry.Entity.ModuleId, string.Join("; ", errors));

			return Results.BadRequest(new RegisterModuleResponse
			{
				Success = false,
				ModuleId = entry.Entity.ModuleId,
				Message = $"Module '{entry.Entity.ModuleId}' failed validation.",
				Errors = errors
			});
		}

		// Success
		logger.LogInformation(
			"Module '{ModuleId}' registered successfully from {BaseUrl}",
			entry.Entity.ModuleId, request.Url);

		return Results.Ok(new RegisterModuleResponse
		{
			Success = true,
			ModuleId = entry.Entity.ModuleId,
			DisplayName = entry.Entity.DisplayName,
			Message = $"Module '{entry.Entity.ModuleId}' registered successfully."
		});
	}

	private static IResult HandleGetModules(ModuleCatalog catalog)
	{
		var modules = catalog.GetAll()
			.Select(e => new ModuleListItem
			{
				ModuleId = e.Entity.ModuleId,
				DisplayName = e.Entity.DisplayName,
				BaseUrl = e.Entity.BaseUrl,
				Enabled = e.Entity.Enabled,
				IsAvailable = e.IsAvailable,
				State = e.State.ToString(),
				RegisteredAt = e.Entity.RegisteredAt
			})
			.ToList();

		return Results.Ok(modules);
	}

	private static IResult HandleGetModule(string moduleId, ModuleCatalog catalog)
	{
		var entry = catalog.Get(moduleId);

		if (entry is null)
		{
			return Results.NotFound(new { error = $"Module '{moduleId}' not found." });
		}

		return Results.Ok(new
		{
			entry.Entity.ModuleId,
			entry.Entity.DisplayName,
			entry.Entity.BaseUrl,
			entry.Entity.Enabled,
			entry.IsAvailable,
			State = entry.State.ToString(),
			entry.Entity.RegisteredAt,
			entry.Entity.UpdatedAt,
			Manifest = entry.Manifest
		});
	}

	private static async Task<IResult> HandleRemoveModule(string moduleId, ModuleCatalog catalog)
	{
		var removed = await catalog.RemoveAsync(moduleId);

		if (!removed)
		{
			return Results.NotFound(new { error = $"Module '{moduleId}' not found." });
		}

		return Results.Ok(new { message = $"Module '{moduleId}' removed." });
	}
}
