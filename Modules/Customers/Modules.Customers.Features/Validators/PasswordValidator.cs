namespace Modules.Customers.Features.Validators;

public class PasswordValidator
{
    /// <summary>
    /// Validates if the provided password meets strong password criteria.
    /// <para>Strong password criteria:
    /// password must be at least 12 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.</para>
    /// </summary>
    /// <param name="password"></param>
    /// <param name="error"></param>
    /// <returns>method returns true if the <paramref name="password"/> is strong, otherwise returns false and sets the <paramref name="error"/></returns>
    public bool IsStrongPassword(string password, out string error)
    {
        try
        {
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long.");

            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter.");

            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least one digit.");

            if (!password.Any(c => "!@#$%^&*()-_=+[]{}|;:',.<>?/`~".Contains(c)))
                errors.Add("Password must contain at least one special character.");

            error = string.Join(" | ", errors);

            return errors.Count == 0;
        }
        catch (Exception ex)
        {
            error = $"An error occurred while validating the password: {ex.Message}";
            return false;
        }
    }
}
