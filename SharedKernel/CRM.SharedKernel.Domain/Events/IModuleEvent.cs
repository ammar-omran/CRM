namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Marker interface for module events exchanged between modules through the platform.
/// The platform-wide event name is declared via <see cref="ModuleEventAttribute"/>.
/// </summary>
public interface IModuleEvent;
