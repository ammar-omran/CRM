using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using CRM.SharedKernel.Domain;

namespace Modules.Customers.Domain.Entities;

public class Customer : IdentityUser, IAuditableEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Hide base PhoneNumber (string) to keep rich PhoneNumber value object
    public new PhoneNumber PhoneNumber { get; set; } = new PhoneNumber("1234567890");

    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = false;

    [MaxLength(10)]
    public string OTP { get; set; } = string.Empty;

    public DateTime OTPCreatedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string HashedEmail { get; set; } = string.Empty;

    public short FailedAttempts { get; set; } = 0;

    public bool IsLocked { get; set; } = false;

    // Identity navigation collections
    public ICollection<CustomerClaim> Claims { get; set; } = null!;
    public ICollection<CustomerUserRole> UserRoles { get; set; } = null!;
    public ICollection<CustomerLogin> UserLogins { get; set; } = null!;
    public ICollection<CustomerToken> UserTokens { get; set; } = null!;
}

/// <summary>
/// Represents a phone number with an optional country code.
/// </summary>
public class PhoneNumber(string number, string countryCode = "")
{
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must contain only digits and be between 10 and 15 digits long.")]
    public string Number { get; } = number;

    [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "Country code must start with '+' followed by 1 to 4 digits.")]
    public string CountryCode { get; } = countryCode;

    public override string ToString() => CountryCode + Number;

    public override bool Equals(object? obj)
    {
        if (obj is PhoneNumber other)
            return Number == other.Number && CountryCode == other.CountryCode;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Number, CountryCode);
}
