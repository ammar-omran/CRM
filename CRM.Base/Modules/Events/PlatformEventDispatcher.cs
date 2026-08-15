using System.Net.Http.Json;
using CRM.Base.Modules.Persistence;
using CRM.SharedKernel.Domain.Events;

namespace CRM.Base.Modules.Events;

/// <summary>
/// Default <see cref="IPlatformEventDispatcher"/> that forwards envelopes to subscribed modules
/// over their <c>/internal/events</c> endpoint.
/// </summary>
public sealed class PlatformEventDispatcher(
	ModuleCatalog catalog,
	IHttpClientFactory httpClientFactory,
	ILogger<PlatformEventDispatcher> logger)
	: IPlatformEventDispatcher
{
	/// <inheritdoc />
	public async Task<EventDeliveryReport> DispatchAsync(
		ModuleEventEnvelope envelope,
		CancellationToken cancellationToken = default)
	{
		var subscribers = catalog.GetAll()
			.Where(e => e.IsAvailable
				&& e.Manifest?.Events?.Subscribed?.Contains(envelope.EventName) == true)
			.ToList();

		if (subscribers.Count == 0)
		{
			logger.LogInformation("Event {EventName} has no available subscribers.", envelope.EventName);
			return new EventDeliveryReport(envelope.EventName, [], 0, []);
		}

		var delivered = 0;
		var failed = new List<string>();

		foreach (var subscriber in subscribers)
		{
			var moduleId = subscriber.Entity.ModuleId;

			try
			{
				var client = httpClientFactory.CreateClient();
				client.BaseAddress = new Uri(subscriber.Entity.BaseUrl.TrimEnd('/') + "/");

				using var response = await client.PostAsJsonAsync("internal/events", envelope, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					delivered++;
					logger.LogDebug("Delivered event {EventName} to {ModuleId}.", envelope.EventName, moduleId);
				}
				else
				{
					failed.Add(moduleId);
					logger.LogWarning(
						"Event {EventName} rejected by {ModuleId}: {StatusCode}.",
						envelope.EventName, moduleId, (int)response.StatusCode);
				}
			}
			catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
			{
				failed.Add(moduleId);
				logger.LogWarning(
					ex,
					"Event {EventName} could not be delivered to {ModuleId}.",
					envelope.EventName, moduleId);
			}
		}

		return new EventDeliveryReport(
			envelope.EventName,
			subscribers.Select(s => s.Entity.ModuleId).ToList(),
			delivered,
			failed);
	}
}
