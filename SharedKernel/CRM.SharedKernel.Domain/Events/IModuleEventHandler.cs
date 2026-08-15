namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Handles a module event received by the module.
/// </summary>
public interface IModuleEventHandler<in TEvent>
    where TEvent : IModuleEvent
{
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}
