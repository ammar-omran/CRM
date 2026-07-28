using System.ComponentModel.DataAnnotations;

namespace CRM.SharedKernel.API.Validators;

[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class NoDuplicatesAttribute(string propertyName) : ValidationAttribute
{
	private readonly string _propertyName = propertyName;

	protected override ValidationResult IsValid(object value, ValidationContext context)
	{
		if (value is not IEnumerable<object> list)
			return ValidationResult.Success;

		var items = list.ToList();

		var duplicates = items
			.GroupBy(item => GetPropertyValue(item, _propertyName))
			.Where(g => g.Key != null && g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Count == 0)
		{
			return new ValidationResult(
				$"Duplicate values found for '{_propertyName}': {string.Join(", ", duplicates)}.");
		}

		return ValidationResult.Success;
	}

	private static object GetPropertyValue(object obj, string propertyName) =>
		obj.GetType().GetProperty(propertyName)?.GetValue(obj);
}
