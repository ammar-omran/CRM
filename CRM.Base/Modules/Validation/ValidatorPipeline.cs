using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Executes all registered validators against a module manifest.
/// The pipeline is extensible — new validators can be added without modifying existing code.
/// </summary>
public sealed class ValidatorPipeline
{
	private readonly IEnumerable<IModuleValidator> _validators;

	/// <summary>
	/// Initializes the pipeline with the provided validators.
	/// </summary>
	/// <param name="validators">The ordered collection of validators to execute.</param>
	public ValidatorPipeline(IEnumerable<IModuleValidator> validators)
	{
		_validators = validators;
	}

	/// <summary>
	/// Runs all validators against the manifest and returns a combined report.
	/// Stops on the first validator that produces errors (fail-fast).
	/// </summary>
	/// <param name="manifest">The manifest to validate.</param>
	/// <returns>The combined validation report.</returns>
	public ValidationReport Validate(IModuleManifest manifest)
	{
		ArgumentNullException.ThrowIfNull(manifest);

		var report = new ValidationReport();

		foreach (var validator in _validators)
		{
			validator.Validate(manifest, report);

			if (!report.IsValid)
			{
				break;
			}
		}

		return report;
	}
}
