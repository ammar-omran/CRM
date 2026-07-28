using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.API.Models.Requests;

public class ResendEmailOtpRequest
{
    [Required]
    public string HashedEmail { get; set; } = string.Empty;
}
