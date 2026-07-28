using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs
{
    public class CustomerLoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

        public CustomerLoginResponseDTO(bool success, string message, string? token = null)
        {
            Success = success;
            Message = message;
            Token = token;
        }

        public CustomerLoginResponseDTO(bool success, string message, string? token, int? customerId, string? customerName, string? customerEmail)
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
