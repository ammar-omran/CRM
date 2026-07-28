using Modules.Customers.Features.Validators;
using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.API.Models.Attributes
{
    /// <summary>
    /// Validation attribute to ensure a password meets strong password requirements.
    /// Uses <see cref="PasswordValidator.IsStrongPassword(string, out string)"/> for validation.
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private readonly PasswordValidator _passwordValidator = new();

        /// <summary>
        /// Determines whether the specified value is a valid strong password.
        /// applying a <see cref="PasswordValidator.IsStrongPassword"/> logic
        /// </summary>
        /// <param name="value">The value of the object to validate (expected to be a string password).</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> if the password is strong; otherwise, a <see cref="ValidationResult"/> with an error message. 
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            try
            {
                string password = value as string ?? "";
                if (_passwordValidator.IsStrongPassword(password, out var error))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(error);
            }
            catch
            {
                return new ValidationResult("An error occurred while validating the password.");
            }
        }
    }
}
