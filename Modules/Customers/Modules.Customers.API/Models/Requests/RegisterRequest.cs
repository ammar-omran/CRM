using Modules.Customers.API.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.API.Models.Requests;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    public required RegisterRequestPhoneNumber PhoneNumber { get; set; }
    [Required]
    [MaxLength(150)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestPhoneNumber(string number, string countryCode = "") // optional country code
{
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must contain only digits and be between 10 and 15 digits long.")]
    public string Number { get; } = number;

    [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "Country code must start with '+' followed by 1 to 4 digits.")]
    public string CountryCode { get; } = countryCode;
}
