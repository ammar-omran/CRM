namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Represents how a module participates in the platform.
/// </summary>
public enum ModuleKind
{
	/// <summary>
	/// Exposes a user-facing interface (UI + API). Entry point for users.
	/// </summary>
	Portal,

	/// <summary>
	/// Provides business logic and data. API-only, no frontend.
	/// </summary>
	Domain,

	/// <summary>
	/// Infrastructure module that other modules depend on. Cannot be disabled.
	/// </summary>
	Platform,

	/// <summary>
	/// Connects to external systems. Acts as a bridge.
	/// </summary>
	Integration
}
