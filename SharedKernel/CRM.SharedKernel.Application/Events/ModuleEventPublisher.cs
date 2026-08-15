using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Events;

namespace CRM.SharedKernel.Application.Events;

/// <summary>
/// Publishes module events by forwarding an envelope to the platform's ingest endpoint.
/// The platform is responsible for delivering the event to subscribed modules.
/// </summary>
public sealed class ModuleEventPublisher(
	IHttpClientFactory httpClientFactory,
	IConfiguration configuration,
	ILogger<ModuleEventPublisher> logger)
	: IModuleEventPublisher
{
	private const string PlatformBaseUrlConfigKey = "InternalEvents:PlatformBaseUrl";

	/// <inheritdoc />
	public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
		where TEvent : IModuleEvent
	{
		var eventName = ModuleEventName.Of(typeof(TEvent));

		var baseUrl = configuration[PlatformBaseUrlConfigKey]
			?? throw new InvalidOperationException(
				$"Configuration key '{PlatformBaseUrlConfigKey}' is not set. The module cannot reach the platform to publish events.");

		var envelope = new ModuleEventEnvelope(
			eventName,
			Guid.NewGuid(),
			DateTime.UtcNow,
			JsonSerializer.SerializeToElement(@event));

		var client = httpClientFactory.CreateClient();
		client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

		using var request = await client.PostAsJsonAsync("internal/events/publish", envelope, cancellationToken);

		if (!request.IsSuccessStatusCode)
		{
			logger.LogError(
				"Failed to publish module event {EventName}: platform returned {StatusCode}.",
				eventName, (int)request.StatusCode);
			return;
		}

		logger.LogDebug("Published module event {EventName} ({EventId}).", eventName, envelope.EventId);
	}
}
