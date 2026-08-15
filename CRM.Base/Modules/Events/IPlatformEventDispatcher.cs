using CRM.SharedKernel.Domain.Events;

namespace CRM.Base.Modules.Events;

/// <summary>
/// Delivers a module event envelope to every registered module that subscribes to it.
/// The platform only understands topology (event name → subscribers), never the business meaning.
/// </summary>
public interface IPlatformEventDispatcher
{
	/// <summary>
	/// Delivers the envelope to all available subscribers of the event.
	/// </summary>
	/// <param name="envelope">The event envelope to deliver.</param>
	/// <param name="cancellationToken">A token to cancel the delivery.</param>
	/// <returns>The delivery report.</returns>
	Task<EventDeliveryReport> DispatchAsync(ModuleEventEnvelope envelope, CancellationToken cancellationToken = default);
}

/// <summary>
/// The result of delivering a module event to subscribed modules.
/// </summary>
public sealed record EventDeliveryReport(
	string EventName,
	IReadOnlyList<string> Subscribers,
	int Delivered,
	IReadOnlyList<string> FailedSubscribers);
