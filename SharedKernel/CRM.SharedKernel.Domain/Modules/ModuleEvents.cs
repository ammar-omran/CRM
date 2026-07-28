namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents the event contributions of a module.
/// Optional — modules that do not participate in event-driven communication do not need this section.
/// </summary>
public sealed record ModuleEvents
{
	/// <summary>
	/// Event IDs this module publishes. The platform uses this for event topology visualization.
	/// </summary>
	public IReadOnlyList<string>? Published { get; init; }

	/// <summary>
	/// Event IDs this module subscribes to. The platform uses this for dependency mapping.
	/// </summary>
	public IReadOnlyList<string>? Subscribed { get; init; }
}
