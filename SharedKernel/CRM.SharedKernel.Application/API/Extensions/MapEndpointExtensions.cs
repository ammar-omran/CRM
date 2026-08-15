using System.Diagnostics;
using System.Text.Json;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering and mapping endpoints in an ASP.NET Core application.
/// </summary>
public static class MapEndpointExtensions
{
	/// <summary>
	/// Registers all SharedKernel.API.endpoints implementing <see cref="IApiEndpoint"/> from the assembly containing the specified type <typeparamref name="T"/> into the service collection.
	/// </summary>
	/// <param name="marker">A type whose assembly will be scanned for <see cref="IApiEndpoint"/> implementations.</param>
	/// <param name="services">The service collection to register the endpoints into.</param>
	/// <returns>The modified service collection with registered endpoints.</returns>
	public static IServiceCollection RegisterApiEndpointsFromAssemblyContaining(this IServiceCollection services, Type marker)
	{
		var assembly = marker.Assembly;

		var endpointTypes = assembly.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(IApiEndpoint)) && t is { IsClass: true, IsAbstract: false, IsInterface: false });

		var serviceDescriptors = endpointTypes
			.Select(type => ServiceDescriptor.Transient(typeof(IApiEndpoint), type))
			.ToArray();

		services.TryAddEnumerable(serviceDescriptors);
		return services;
	}

	/// <summary>
	/// Maps all registered SharedKernel.API.endpoints implementing the <see cref="IApiEndpoint"/> interface to the specified web application.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> instance to which the endpoints will be mapped.</param>
	/// <returns>The same <see cref="WebApplication"/> instance to allow for method chaining.</returns>
	public static WebApplication MapApiEndpoints(this WebApplication app)
	{
		var endpoints = app.Services.GetRequiredService<IEnumerable<IApiEndpoint>>();

		foreach (var endpoint in endpoints)
		{
			endpoint.MapEndpoint(app);
		}

		return app;
	}

	public static WebApplication MapDefaultEndpoints(this WebApplication app)
	{
		// Adding health checks endpoints to applications in non-development environments has security implications.
		// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
		if (app.Environment.IsDevelopment())
		{
			// All health checks must pass for app to be considered ready to accept traffic after starting
			app.MapHealthChecks("/health");

			// Only health checks tagged with the "live" tag must pass for app to be considered alive
			app.MapHealthChecks("/alive", new HealthCheckOptions
			{
				Predicate = r => r.Tags.Contains("live")
			});
		}

		app.MapGet("/module/manifest", (IServiceProvider services) =>
		{
			var manifest = services.GetService<IModuleManifest>();

			return manifest is null
				? Results.Problem(
					detail: "No module manifest has been registered.",
					statusCode: StatusCodes.Status500InternalServerError)
				: Results.Ok(manifest);
		});

		// Receives module events delivered by the platform and dispatches them to the
		// matching local handlers by event name (strategy pattern).
		app.MapPost("/internal/events", HandleInternalEvent);

		return app;
	}

	private static async Task<IResult> HandleInternalEvent(
		ModuleEventEnvelope envelope,
		IEnumerable<IModuleEventHandler> handlers,
		ILoggerFactory loggerFactory,
		CancellationToken cancellationToken)
	{
		var logger = loggerFactory.CreateLogger("InternalEvents");

		var activity = Activity.Current;
		activity?.SetTag("module.event", envelope.EventName);
		activity?.SetTag("module.event.id", envelope.EventId);

		var matchingHandlers = handlers
			.Where(h => h.EventName.Equals(envelope.EventName, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (matchingHandlers.Count == 0)
		{
			logger.LogWarning("Ignoring event {EventName}: no handler registered.", envelope.EventName);
			return Results.NotFound(new { error = $"No handler is registered for event '{envelope.EventName}'." });
		}

		foreach (var handler in matchingHandlers)
		{
			try
			{
				await handler.HandleAsync(envelope.Payload, cancellationToken);
			}
			catch (JsonException ex)
			{
				logger.LogWarning(
					ex,
					"Ignoring event {EventName}: payload could not be deserialized by {HandlerType}.",
					envelope.EventName, handler.GetType().Name);
				return Results.BadRequest(new { error = $"Payload could not be deserialized for event '{envelope.EventName}'." });
			}
		}

		// Record the handled event on the receiving span (like an exception) so it
		// appears in the span's Events panel in Aspire.
		Activity.Current?.AddEvent(new ActivityEvent(
			name: envelope.EventName,
			timestamp: DateTimeOffset.UtcNow,
			tags: new ActivityTagsCollection
			{
				["module.event.id"] = envelope.EventId.ToString(),
				["module.event.handlers"] = matchingHandlers.Count.ToString(),
				["module.event.outcome"] = "handled"
			}));

		return Results.Ok(new { envelope.EventName, envelope.EventId });
	}
}
