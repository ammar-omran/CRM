using System.Collections.Concurrent;
using CRM.Base.Modules.State;

namespace CRM.Base.Modules.Registry;

/// <summary>
/// Thread-safe runtime storage for registered modules.
/// Uses ConcurrentDictionary to support concurrent registration scenarios.
/// </summary>
public sealed class ModuleRegistry : IModuleRegistry
{
	private readonly ConcurrentDictionary<string, RegisteredModule> _modules = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc />
	public void Register(RegisteredModule module)
	{
		ArgumentNullException.ThrowIfNull(module);

		if (!_modules.TryAdd(module.ModuleId, module))
		{
			throw new InvalidOperationException(
				$"Module '{module.ModuleId}' is already registered. " +
				"Remove the existing module before re-registering.");
		}
	}

	/// <inheritdoc />
	public bool Remove(string moduleId)
	{
		return _modules.TryRemove(moduleId, out _);
	}

	/// <inheritdoc />
	public RegisteredModule? Get(string moduleId)
	{
		_modules.TryGetValue(moduleId, out var module);
		return module;
	}

	/// <inheritdoc />
	public IReadOnlyList<RegisteredModule> GetAll()
	{
		return _modules.Values.ToList().AsReadOnly();
	}

	/// <inheritdoc />
	public IReadOnlyList<RegisteredModule> GetByState(ModuleState state)
	{
		return _modules.Values
			.Where(m => m.State == state)
			.ToList()
			.AsReadOnly();
	}

	/// <inheritdoc />
	public IReadOnlyList<RegisteredModule> GetByCapability(string capabilityId)
	{
		return _modules.Values
			.Where(m => m.State != ModuleState.Removed && m.State != ModuleState.Failed)
			.Where(m => m.Manifest.ProvidedCapabilities
				.Any(c => c.CapabilityId.Equals(capabilityId, StringComparison.OrdinalIgnoreCase)))
			.ToList()
			.AsReadOnly();
	}

	/// <inheritdoc />
	public bool Exists(string moduleId)
	{
		return _modules.ContainsKey(moduleId);
	}

	/// <inheritdoc />
	public RegisteredModule? UpdateState(string moduleId, ModuleState state)
	{
		if (!_modules.TryGetValue(moduleId, out var module))
		{
			return null;
		}

		module.State = state;

		switch (state)
		{
			case ModuleState.Running:
				module.StartedAtUtc = DateTime.UtcNow;
				break;
			case ModuleState.Disabled:
				module.DisabledAtUtc = DateTime.UtcNow;
				break;
		}

		return module;
	}

	/// <inheritdoc />
	public RegisteredModule? MarkFailed(string moduleId, string reason, string? details = null)
	{
		if (!_modules.TryGetValue(moduleId, out var module))
		{
			return null;
		}

		module.State = ModuleState.Failed;
		module.FailureReason = reason;
		module.FailureDetails = details;
		module.FailedAtUtc = DateTime.UtcNow;

		return module;
	}
}
