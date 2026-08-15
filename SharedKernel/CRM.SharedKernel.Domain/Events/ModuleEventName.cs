using System.Collections.Concurrent;
using System.Reflection;

namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// Resolves the platform event name declared on an event type via <see cref="ModuleEventAttribute"/>.
/// Falls back to the type name when no attribute is present.
/// </summary>
public static class ModuleEventName
{
	private static readonly ConcurrentDictionary<Type, string> Cache = new();

	/// <summary>
	/// Gets the platform event name for the given event type.
	/// </summary>
	/// <param name="eventType">The event DTO type.</param>
	/// <returns>The declared dotted event name, or the type name if undeclared.</returns>
	public static string Of(Type eventType)
		=> Cache.GetOrAdd(eventType, static t => t.GetCustomAttribute<ModuleEventAttribute>()?.Name ?? t.Name);
}
