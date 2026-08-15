namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Publishes a module event so the platform can deliver it to subscribed modules.
/// </summary>
public interface IModuleEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IModuleEvent;
}
