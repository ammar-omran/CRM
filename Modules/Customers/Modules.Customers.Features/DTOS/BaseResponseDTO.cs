namespace Modules.Customers.Features.DTOs
{
    public class BaseResponseDTO(bool status, string message)
    {
        public bool Status { get; set; } = status;
        public string Message { get; set; } = message;
    }
}
