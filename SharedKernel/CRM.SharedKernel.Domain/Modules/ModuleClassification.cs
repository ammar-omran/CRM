namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// Classifies a module by how it participates in the platform (Kind)
/// and who it serves (Audience).
/// </summary>
public sealed record ModuleClassification
{
	/// <summary>
	/// How this module participates in the platform.
	/// </summary>
	public required ModuleKind Kind { get; init; }

	/// <summary>
	/// Who this module serves.
	/// </summary>
	public required ModuleAudience Audience { get; init; }
}
