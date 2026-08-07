using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    public required PhoneNumber PhoneNumber { get; set; } = new PhoneNumber("123-456-789");
    [MaxLength(150)]
    public string Password { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdateDate { get; set; } = DateTime.Now;
    [Length(10, 10)]
    public string OTP { get; set; } = string.Empty;
    public DateTime OTPCreatedDate { get; set; } = DateTime.Now;
    public string HashedEmail { get; set; } = string.Empty;
    public Int16 FailedAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;
}

/// <summary>
/// Represents a phone number with an optional country code.
/// </summary>
/// <param name="number">The phone number digits.</param>
/// <param name="countryCode">The country code, starting with '+'.</param>
public class PhoneNumber(string number, string countryCode = "") // optional country code
{
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must contain only digits and be between 10 and 15 digits long.")]
    public string Number { get; } = number;

    [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "Country code must start with '+' followed by 1 to 4 digits.")]
    public string CountryCode { get; } = countryCode;
    public override string ToString() => string.Join(CountryCode, Number);

    public override bool Equals(object? obj)
    {
        if (obj is PhoneNumber other)
        {
            return Number == other.Number && CountryCode == other.CountryCode;
        }
        return false;
    }
}
