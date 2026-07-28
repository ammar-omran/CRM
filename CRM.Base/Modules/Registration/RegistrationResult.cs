using CRM.Base.Modules.Registry;
using CRM.Base.Modules.Validation;

namespace CRM.Base.Modules.Registration;

/// <summary>
/// The outcome of a module registration attempt.
/// Contains rich diagnostics for debugging and admin UI display.
/// </summary>
public sealed class RegistrationResult
{
	/// <summary>
	/// Whether the registration succeeded.
	/// </summary>
	public bool IsSuccess { get; init; }

	/// <summary>
	/// The registered module, or null if registration failed.
	/// </summary>
	public RegisteredModule? Module { get; init; }

	/// <summary>
	/// The validation report from the pipeline. Null if validation was not performed.
	/// </summary>
	public ValidationReport? ValidationReport { get; init; }

	/// <summary>
	/// Human-readable summary of the registration outcome.
	/// </summary>
	public string Message { get; init; } = string.Empty;

	/// <summary>
	/// The pipeline stage where registration stopped (or completed).
	/// </summary>
	public string CompletedStage { get; init; } = string.Empty;

	/// <summary>
	/// Execution duration of the entire registration pipeline.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <summary>
	/// Creates a successful registration result.
	/// </summary>
	/// <param name="module">The successfully registered module.</param>
	/// <param name="validationReport">The validation report.</param>
	/// <param name="duration">The pipeline execution duration.</param>
	public static RegistrationResult Success(RegisteredModule module, ValidationReport validationReport, TimeSpan duration)
	{
		return new RegistrationResult
		{
			IsSuccess = true,
			Module = module,
			ValidationReport = validationReport,
			Message = $"Module '{module.ModuleId}' registered successfully.",
			CompletedStage = "Activated",
			Duration = duration
		};
	}

	/// <summary>
	/// Creates a failed registration result.
	/// </summary>
	/// <param name="moduleId">The module ID that failed.</param>
	/// <param name="stage">The stage where failure occurred.</param>
	/// <param name="message">The failure message.</param>
	/// <param name="validationReport">Optional validation report if failure occurred during validation.</param>
	/// <param name="duration">The pipeline execution duration.</param>
	public static RegistrationResult Failure(
		string moduleId,
		string stage,
		string message,
		ValidationReport? validationReport = null,
		TimeSpan duration = default)
	{
		return new RegistrationResult
		{
			IsSuccess = false,
			ValidationReport = validationReport,
			Message = $"Module '{moduleId}' registration failed at stage '{stage}': {message}",
			CompletedStage = stage,
			Duration = duration
		};
	}
}
