using System.ComponentModel.DataAnnotations;

namespace Modules.Customers.API.Models.Requests
{
    public class LoginWithPhoneRequest
    {

        [Required]
        public RegisterRequestPhoneNumber PhoneNumber { get; set; } = new RegisterRequestPhoneNumber("", "");

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}


