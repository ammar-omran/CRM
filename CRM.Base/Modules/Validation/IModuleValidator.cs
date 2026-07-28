using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.Validation;

/// <summary>
/// Validates a module manifest during the registration pipeline.
/// Each validator has a single responsibility and produces a validation report.
/// </summary>
public interface IModuleValidator
{
	/// <summary>
	/// The name of this validator for diagnostic purposes.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Validates the given manifest and populates the validation report.
	/// </summary>
	/// <param name="manifest">The manifest to validate.</param>
	/// <param name="report">The report to populate with errors and warnings.</param>
	void Validate(IModuleManifest manifest, ValidationReport report);
}
