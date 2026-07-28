namespace Modules.Customers.Features.DTOs;

/// <summary>
/// Data transfer object for customer phone-number/password login.
/// </summary>
public class CustomerLoginWithPhoneDTO
{
    /// <summary>The customer's phone number (digits only, no country code).</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>The country code including the '+' prefix (e.g. "+20").</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>The customer's plain-text password.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The User-Agent string from the HTTP request header, identifying the
    /// device and browser used to log in. Included in the login notification email.
    /// </summary>
    public string DeviceInfo { get; set; } = "Unknown Device";
}
