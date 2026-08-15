using CRM.Base.Modules.Events;
using CRM.SharedKernel.Domain.Events;

namespace CRM.Base.Modules.Api;

/// <summary>
/// Internal endpoints used by modules to communicate through the platform.
/// </summary>
public static class InternalEndpoints
{
	/// <summary>
	/// Maps the internal module-to-platform endpoints.
	/// </summary>
	/// <param name="app">The web application.</param>
	/// <returns>The web application for chaining.</returns>
	public static WebApplication MapInternalEndpoints(this WebApplication app)
	{
		// Ingest point for events published by modules.
		app.MapPost("/internal/events/publish", HandlePublishEvent);

		return app;
	}

	private static async Task<IResult> HandlePublishEvent(
		ModuleEventEnvelope envelope,
		IPlatformEventDispatcher dispatcher,
		ILoggerFactory loggerFactory,
		CancellationToken cancellationToken)
	{
		var logger = loggerFactory.CreateLogger("InternalEvents");

		logger.LogInformation("Received module event {EventName} ({EventId}).", envelope.EventName, envelope.EventId);

		var report = await dispatcher.DispatchAsync(envelope, cancellationToken);

		return Results.Ok(new
		{
			envelope.EventName,
			envelope.EventId,
			report.Subscribers,
			report.Delivered,
			report.FailedSubscribers
		});
	}
}
