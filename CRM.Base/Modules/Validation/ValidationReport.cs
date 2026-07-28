namespace CRM.Base.Modules.Validation;

/// <summary>
/// Represents the outcome of validating a module manifest.
/// </summary>
public sealed class ValidationReport
{
	/// <summary>
	/// Whether all validation checks passed.
	/// </summary>
	public bool IsValid => !Errors.Any();

	/// <summary>
	/// Validation errors that prevent registration.
	/// </summary>
	public List<ValidationError> Errors { get; } = [];

	/// <summary>
	/// Non-blocking warnings that do not prevent registration.
	/// </summary>
	public List<ValidationWarning> Warnings { get; } = [];

	/// <summary>
	/// Adds an error to the report.
	/// </summary>
	/// <param name="validator">The validator that produced the error.</param>
	/// <param name="message">The error message.</param>
	/// <param name="code">An optional error code for programmatic handling.</param>
	public void AddError(string validator, string message, string? code = null)
	{
		Errors.Add(new ValidationError(validator, message, code));
	}

	/// <summary>
	/// Adds a warning to the report.
	/// </summary>
	/// <param name="validator">The validator that produced the warning.</param>
	/// <param name="message">The warning message.</param>
	public void AddWarning(string validator, string message)
	{
		Warnings.Add(new ValidationWarning(validator, message));
	}
}

/// <summary>
/// Represents a validation error that prevents module registration.
/// </summary>
public sealed record ValidationError(string Validator, string Message, string? Code);

/// <summary>
/// Represents a non-blocking validation warning.
/// </summary>
public sealed record ValidationWarning(string Validator, string Message);
