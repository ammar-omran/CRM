using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.API.Models.Requests;

public class ConfirmEmailOtpRequest
{
    [Required]
    public string HashedEmail { get; set; } = string.Empty;

    [Required]
    [Length(6, 6)]
    public string Otp { get; set; } = string.Empty;
}
