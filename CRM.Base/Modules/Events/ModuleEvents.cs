using CRM.SharedKernel.Domain.Events;

namespace CRM.Base.Modules.Events;

/// <summary>
/// Published when a module is discovered by the platform.
/// </summary>
public sealed record ModuleDiscoveredEvent(string ModuleId, string ModuleVersion) : IEvent;

/// <summary>
/// Published when a module passes all validation checks.
/// </summary>
public sealed record ModuleValidatedEvent(string ModuleId, string ModuleVersion) : IEvent;

/// <summary>
/// Published when a module is registered in the runtime registry.
/// </summary>
public sealed record ModuleRegisteredEvent(string ModuleId, string ModuleVersion, DateTime RegisteredAtUtc) : IEvent;

/// <summary>
/// Published when a module begins initialization.
/// </summary>
public sealed record ModuleInitializingEvent(string ModuleId) : IEvent;

/// <summary>
/// Published when a module reaches running state.
/// </summary>
public sealed record ModuleStartedEvent(string ModuleId, DateTime StartedAtUtc) : IEvent;

/// <summary>
/// Published when a module is disabled.
/// </summary>
public sealed record ModuleDisabledEvent(string ModuleId, DateTime DisabledAtUtc) : IEvent;

/// <summary>
/// Published when a module is removed from the platform.
/// </summary>
public sealed record ModuleRemovedEvent(string ModuleId, DateTime RemovedAtUtc) : IEvent;

/// <summary>
/// Published when module registration fails at any stage.
/// </summary>
public sealed record ModuleRegistrationFailedEvent(string ModuleId, string Reason, string? Details) : IEvent;
