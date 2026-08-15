using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CRM.SharedKernel.Domain.Events;

namespace CRM.SharedKernel.Application.Events;

/// <summary>
/// Maps a platform event name to the concrete event DTO type this module registers a handler for.
/// Built from the registered <see cref="IModuleEventHandler{TEvent}"/> services, so the platform
/// delivers raw payloads and the module recreates its own local event representation.
/// </summary>
public sealed class ModuleEventHandlerRegistry(IEnumerable<ServiceDescriptor> descriptors)
{
	private readonly IReadOnlyDictionary<string, Type> _eventTypes = BuildMap(descriptors);

	private static Dictionary<string, Type> BuildMap(IEnumerable<ServiceDescriptor> descriptors)
	{
		var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		foreach (var descriptor in descriptors)
		{
			if (descriptor.ServiceType is not { IsGenericType: true } serviceType
				|| serviceType.GetGenericTypeDefinition() != typeof(IModuleEventHandler<>))
			{
				continue;
			}

			var eventType = serviceType.GetGenericArguments()[0];
			var attribute = eventType.GetCustomAttribute<ModuleEventAttribute>();

			if (attribute is null)
			{
				continue;
			}

			map.TryAdd(attribute.Name, eventType);
		}

		return map;
	}

	/// <summary>
	/// Resolves the concrete event DTO type registered for the given event name.
	/// </summary>
	/// <param name="eventName">The platform event name.</param>
	/// <param name="eventType">The concrete event DTO type, if registered.</param>
	/// <returns>True when a local event type is registered for the event name.</returns>
	public bool TryResolve(string eventName, out Type eventType)
		=> _eventTypes.TryGetValue(eventName, out eventType!);
}
