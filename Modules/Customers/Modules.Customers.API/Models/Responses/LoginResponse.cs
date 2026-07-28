namespace Modules.Customers.API.Models.Responses
{
    public class LoginResponse
    {
        public bool Success { get; }
        public string Message { get; }
        public string? Token { get; }
        public int? CustomerId { get; }
        public string? CustomerName { get; }
        public string? CustomerEmail { get; }

        public LoginResponse(bool success, string message, string? token = null)
        {
            Success = success;
            Message = message;
            Token = token;
        }

        public LoginResponse(bool success, string message, string? token, int? customerId, string? customerName, string? customerEmail)
        {
            Success = success;
            Message = message;
            Token = token;
            CustomerId = customerId;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
        }
    }
}
