namespace Modules.Customers.Features.DTOS;

public class RegisterResponseDTO
{
    public string MaskedEmail { get; set; } = string.Empty;
    public string HashedEmail { get; set; } = string.Empty;
}
