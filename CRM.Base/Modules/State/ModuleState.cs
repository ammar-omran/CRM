namespace CRM.Base.Modules.State;

/// <summary>
/// Represents the runtime state of a module within the platform.
/// </summary>
public enum ModuleState
{
	/// <summary>
	/// Module has been discovered but not yet validated.
	/// </summary>
	Discovered,

	/// <summary>
	/// Module manifest has been validated successfully.
	/// </summary>
	Validated,

	/// <summary>
	/// Module has been registered in the runtime registry.
	/// </summary>
	Registered,

	/// <summary>
	/// Module is executing its initialization logic.
	/// </summary>
	Initializing,

	/// <summary>
	/// Module is active and serving requests.
	/// </summary>
	Running,

	/// <summary>
	/// Module has been temporarily disabled by an administrator.
	/// </summary>
	Disabled,

	/// <summary>
	/// Module has encountered an unrecoverable error.
	/// </summary>
	Failed,

	/// <summary>
	/// Module has been removed from the platform.
	/// </summary>
	Removed
}
