using CRM.Base.Modules.State;
using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Admin;

/// <summary>
/// Lightweight, read-only projection of a registered module used by the admin UI list.
/// All values are sourced from the existing <see cref="CRM.Base.Modules.Persistence.CatalogEntry"/>.
/// </summary>
public sealed class ModuleSummary
{
	public required string ModuleId { get; init; }
	public required string DisplayName { get; init; }
	public required string BaseUrl { get; init; }
	public bool Enabled { get; init; }
	public bool IsAvailable { get; init; }
	public required ModuleState State { get; init; }
	public DateTime RegisteredAt { get; init; }
	public string? Version { get; init; }
	public ModuleKind Kind { get; init; }
	public ModuleAudience Audience { get; init; }
}

/// <summary>
/// Read-only projection of a single module's manifest + registration information for the
/// generic module details page. Contains no secret or sensitive configuration.
/// </summary>
public sealed class ModuleDetail
{
	public required string ModuleId { get; init; }
	public required string DisplayName { get; init; }
	public string? Description { get; init; }
	public string? Author { get; init; }
	public IReadOnlyList<string>? Tags { get; init; }
	public required string Version { get; init; }
	public required ModuleKind Kind { get; init; }
	public required ModuleAudience Audience { get; init; }
	public required ModuleState State { get; init; }
	public bool Enabled { get; init; }
	public bool IsAvailable { get; init; }
	public string BaseUrl { get; init; } = string.Empty;

	// Integration
	public ModuleApi? Api { get; init; }
	public ModuleHealth? Health { get; init; }
	public ModuleDatabase? Database { get; init; }

	// Capabilities & dependencies
	public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; init; } = [];
	public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; init; }
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; init; }
	public IReadOnlyList<ModulePolicy>? Policies { get; init; }
}

/// <summary>
/// Result of an admin mutation (register / enable / disable / remove).
/// Surfaces the actual backend error text instead of a generic message.
/// </summary>
public sealed class ModuleOperationResult
{
	public bool IsSuccess { get; init; }
	public string? ModuleId { get; init; }
	public string Message { get; init; } = string.Empty;
	public IReadOnlyList<string> Errors { get; init; } = [];

	public static ModuleOperationResult Success(string moduleId, string message) =>
		new() { IsSuccess = true, ModuleId = moduleId, Message = message };

	public static ModuleOperationResult Failure(string? moduleId, string message, IEnumerable<string>? errors = null) =>
		new()
		{
			IsSuccess = false,
			ModuleId = moduleId,
			Message = message,
			Errors = errors?.ToList() ?? []
		};
}
