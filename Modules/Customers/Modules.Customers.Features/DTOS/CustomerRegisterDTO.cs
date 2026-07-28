namespace Modules.Customers.Features.DTOs;

public class CustomerRegisterDTO(string name, string email, CustomerRegisterPhoneNumber phoneNumber, string password)
{
    public string Name { get; set; } = name;
    public string Email { get; set; } = email;
    public CustomerRegisterPhoneNumber PhoneNumber { get; set; } = phoneNumber;
    public string Password { get; set; } = password;
}

public class CustomerRegisterPhoneNumber(string number, string countryCode = "") // optional country code
{
    public string Number { get; } = number;
    public string CountryCode { get; } = countryCode;
}
