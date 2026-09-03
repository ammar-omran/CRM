using CRM.SharedKernel.Domain.Results;
using Microsoft.AspNetCore.Identity;

namespace Modules.Customers.Domain.Errors;

public static class CustomerErrors
{
	private const string Prefix = "Customer";

	public static Error NotFound(int customerId) =>
		Error.NotFound($"{Prefix}.{nameof(NotFound)}", $"Customer with ID {customerId} was not found.");

	public static Error NotFound(string customerId) =>
		Error.NotFound($"{Prefix}.{nameof(NotFound)}", $"Customer with ID {customerId} was not found.");

	public static Error NotFoundByEmail(string email) =>
		Error.NotFound($"{Prefix}.{nameof(NotFound)}", $"Customer with email {email} was not found.");

	public static Error EmailAndPhoneAlreadyExists =>
		Error.Conflict($"{Prefix}.{nameof(EmailAndPhoneAlreadyExists)}", "Email and Phone are already used.");

	public static Error EmailAlreadyExists =>
		Error.Conflict($"{Prefix}.{nameof(EmailAlreadyExists)}", "A customer with this email already exists.");

	public static Error PhoneAlreadyExists =>
		Error.Conflict($"{Prefix}.{nameof(PhoneAlreadyExists)}", "A customer with this phone number already exists.");

	public static Error InvalidCredentials =>
		Error.Unauthorized($"{Prefix}.{nameof(InvalidCredentials)}", "Invalid email or password.");

	public static Error EmailNotVerified =>
		Error.Forbidden($"{Prefix}.{nameof(EmailNotVerified)}", "Email address has not been verified.");

	public static Error CustomerNotActive =>
		Error.Unauthorized($"{Prefix}.{nameof(CustomerNotActive)}", "Customer account is inactive. Please verify your email.");

	public static Error InvalidToken =>
		Error.Unauthorized($"{Prefix}.{nameof(InvalidToken)}", "Invalid token.");

	public static Error LockedOut(DateTimeOffset? lockoutEnd) =>
		Error.Unauthorized($"{Prefix}.{nameof(LockedOut)}", lockoutEnd.HasValue ? $"Account is locked out until {lockoutEnd.Value:O}." : "Account is locked out.");

	public static Error RegistrationFailed(IEnumerable<IdentityError> errors) =>
		Error.Failure($"{Prefix}.{nameof(RegistrationFailed)}", string.Join("; ", errors.Select(e => e.Description)));

	public static Error OtpExpired =>
		Error.Failure($"{Prefix}.{nameof(OtpExpired)}", "The OTP has expired.");

	public static Error OtpInvalid =>
		Error.Failure($"{Prefix}.OtpWrong", "The OTP is invalid.");

	public static Error OtpWrong => OtpInvalid;

	public static Error OtpLocked =>
		Error.Failure($"{Prefix}.{nameof(OtpLocked)}", "Too many failed attempts. Account is locked.");

	public static Error ResendCooldown =>
		Error.Failure($"{Prefix}.{nameof(ResendCooldown)}", "Please wait before requesting another OTP.");

	public static Error AlreadyVerified =>
		Error.Conflict($"{Prefix}.{nameof(AlreadyVerified)}", "Email is already verified.");
}
