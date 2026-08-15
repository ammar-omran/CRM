namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Declares the platform-wide event name for a module event DTO.
/// The name is used for routing and delivery; it must match the name in the manifest's Published/Subscribed lists.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ModuleEventAttribute(string name) : Attribute
{
    /// <summary>
    /// The dotted event name, e.g. "ticketing.ticket-created".
    /// </summary>
    public string Name { get; } = name;
}
