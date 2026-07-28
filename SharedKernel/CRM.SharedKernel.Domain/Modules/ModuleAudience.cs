namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents who the module serves.
/// </summary>
public enum ModuleAudience
{
	/// <summary>
	/// Serves platform administrators or internal operators.
	/// </summary>
	Internal,

	/// <summary>
	/// Serves end users or external consumers.
	/// </summary>
	External,

	/// <summary>
	/// Serves both internal and external users.
	/// </summary>
	Shared
}
