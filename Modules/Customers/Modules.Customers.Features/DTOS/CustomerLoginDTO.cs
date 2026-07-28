namespace Modules.Customers.Features.DTOs;

/// <summary>
/// Data transfer object for customer email/password login.
/// </summary>
public class CustomerLoginDTO
{
    /// <summary>The customer's registered email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The customer's plain-text password (validated before hashing comparison).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The User-Agent string from the HTTP request header, identifying the
    /// device and browser used to log in. Included in the login notification email.
    /// </summary>
    public string DeviceInfo { get; set; } = "Unknown Device";
}
