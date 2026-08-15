using System.Text.Json;

namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Handles a module event received by the module.
/// The event name identifies which events this handler handles; the payload is a
/// raw <see cref="JsonElement"/> that the handler deserializes into its own local event DTO.
/// </summary>
public interface IModuleEventHandler
{
	/// <summary>
	/// The platform event name this handler handles, e.g. "ticketing.ticket-created".
	/// </summary>
	string EventName { get; }

	/// <summary>
	/// Handles the event delivered by the platform.
	/// </summary>
	/// <param name="payload">The raw event payload.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	Task HandleAsync(JsonElement payload, CancellationToken cancellationToken = default);
}
