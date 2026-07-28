using CRM.SharedKernel.Domain.Results;

namespace Modules.Customers.Domain.Errors;

public static class CustomerErrors
{
	public static Error NotFound(int customerId) =>
		Error.NotFound("Customer.NotFound", $"Customer with ID {customerId} was not found.");

	public static Error EmailAndPhoneAlreadyExists =>
		Error.Conflict("Customer.EmailAndPhoneAlreadyExists", "Email and Phone are already used.");

	public static Error EmailAlreadyExists =>
		Error.Conflict("Customer.EmailAlreadyExists", "A customer with this email already exists.");

	public static Error PhoneAlreadyExists =>
		Error.Conflict("Customer.PhoneAlreadyExists", "A customer with this phone number already exists.");

	public static Error InvalidCredentials =>
		Error.Unauthorized("Customer.InvalidCredentials", "Invalid email or password.");

	public static Error EmailNotVerified =>
		Error.Forbidden("Customer.EmailNotVerified", "Email address has not been verified.");

	public static Error OtpExpired =>
		Error.Failure("Customer.OtpExpired", "The OTP has expired.");

	public static Error OtpInvalid =>
		Error.Failure("Customer.OtpInvalid", "The OTP is invalid.");

	public static Error OtpLocked =>
		Error.Failure("Customer.OtpLocked", "Too many failed attempts. Account is locked.");
}
