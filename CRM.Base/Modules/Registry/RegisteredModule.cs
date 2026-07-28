using CRM.Base.Modules.State;
using CRM.Base.Modules.Validation;
using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Registry;

/// <summary>
/// Represents a module hosted by the platform at runtime.
/// Wraps the immutable manifest with mutable runtime information owned by the platform.
/// </summary>
public sealed class RegisteredModule
{
	/// <summary>
	/// The module's immutable manifest contract.
	/// </summary>
	public required IModuleManifest Manifest { get; init; }

	/// <summary>
	/// The current runtime state of this module.
	/// </summary>
	public ModuleState State { get; internal set; } = ModuleState.Discovered;

	/// <summary>
	/// UTC timestamp when this module was registered.
	/// </summary>
	public DateTime? RegisteredAtUtc { get; internal set; }

	/// <summary>
	/// UTC timestamp when this module was last started.
	/// </summary>
	public DateTime? StartedAtUtc { get; internal set; }

	/// <summary>
	/// UTC timestamp when this module was disabled.
	/// </summary>
	public DateTime? DisabledAtUtc { get; internal set; }

	/// <summary>
	/// The validation result from manifest validation.
	/// Null until validation has been performed.
	/// </summary>
	public ValidationReport? ValidationReport { get; internal set; }

	/// <summary>
	/// The error message if the module is in Failed state.
	/// Null when the module has not failed.
	/// </summary>
	public string? FailureReason { get; internal set; }

	/// <summary>
	/// The error details if the module is in Failed state.
	/// </summary>
	public string? FailureDetails { get; internal set; }

	/// <summary>
	/// UTC timestamp when this module failed.
	/// </summary>
	public DateTime? FailedAtUtc { get; internal set; }

	/// <summary>
	/// Gets the module's unique identifier.
	/// Shorthand for Manifest.Identity.ModuleId.
	/// </summary>
	public string ModuleId => Manifest.Identity.ModuleId;

	/// <summary>
	/// Gets the module's display name.
	/// Shorthand for Manifest.Identity.DisplayName.
	/// </summary>
	public string DisplayName => Manifest.Identity.DisplayName;
}
