using System.Text.Json;

namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Base class for typed module event handlers. Bridges the raw <see cref="JsonElement"/>
/// payload received from the platform to a strongly typed local event DTO.
/// </summary>
/// <typeparam name="TEvent">The local event DTO type handled by this handler.</typeparam>
public abstract class ModuleEventHandler<TEvent> : IModuleEventHandler
	where TEvent : IModuleEvent
{
	/// <inheritdoc />
	public string EventName => ModuleEventName.Of(typeof(TEvent));

	/// <inheritdoc />
	public Task HandleAsync(JsonElement payload, CancellationToken cancellationToken = default)
		=> HandleAsync(
			payload.Deserialize<TEvent>()
				?? throw new JsonException($"Payload for event '{EventName}' could not be deserialized into {typeof(TEvent).Name}."),
			cancellationToken);

	/// <summary>
	/// Handles the typed event deserialized from the platform payload.
	/// </summary>
	/// <param name="event">The local event DTO.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	protected abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
