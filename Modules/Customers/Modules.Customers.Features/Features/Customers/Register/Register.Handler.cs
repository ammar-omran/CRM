using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Services;
using Modules.Customers.Domain.Entities;
using Modules.Customers.Domain.Errors;
using Modules.Customers.Infrastructure.Database;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Modules.Customers.Features.Customers.Register;

public sealed record RegisterRequest(
	string Name,
	string Email,
	string PhoneNumber,
	string CountryCode,
	string Password
);

public sealed record RegisterResponse(int Id, string Name, string Email);

internal interface IRegisterHandler : IHandler
{
	Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken);
}

internal sealed class RegisterHandler(
		CustomersDbContext context,
		IEmailSender emailSender,
		IPasswordHasher passwordHasher,
		ILogger<RegisterHandler> logger)
		: IRegisterHandler
{
	public async Task<Result<RegisterResponse>> HandleAsync(
		RegisterRequest request,
		CancellationToken cancellationToken)
	{


				var customer = new Customer
		{
			Name = request.Name,
			Email = request.Email,
			PhoneNumber = new PhoneNumber(request.PhoneNumber, request.CountryCode),
						// Hash the password before saving to the database
						Password = passwordHasher.Hash(request.Password),
			CreatedDate = DateTime.UtcNow,
			UpdateDate = DateTime.UtcNow
		};

		var existingCustomer = await context.Customers
			.FirstOrDefaultAsync(c => c.Email == customer.Email || c.PhoneNumber.Number == customer.PhoneNumber.Number, cancellationToken);

		var uniquenessResult = ValidateCustomerUniqueness(existingCustomer, customer);

		if (uniquenessResult is not null)
		{
			logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
			return uniquenessResult.Value;
		}
		var otp = GenerateOtp();

		string hashedEmail = GetShortHash(request.Email);
		string maskedEmail = Regex.Replace(request.Email, @"(^.).*(?=@)", m =>
			m.Value.Length > 2 ? $"{m.Value[0]}***{m.Value[^1]}" : $"{m.Value[0]}***");


		context.Customers.Add(customer);
		await context.SaveChangesAsync(cancellationToken);

		emailSender.SendAsync(customer.Email,
			$"Welcome to Our Service, OTP: {otp}",
			$"Thank you for registering with us!, your OTP: {otp}, please note that the otp will expire after 1 minute",
			cancellationToken);

		logger.LogInformation("Customer registered: {Email}", request.Email);

		return new RegisterResponse(customer.Id, customer.Name, customer.Email);
	}

	/// <summary>
	/// Generates a random OTP of specified <paramref name="length"/> (default is 6 digits).
	/// </summary>
	/// <param name="length"></param>
	/// <returns></returns>
	public string GenerateOtp(int length = 6)
	{
		var random = new Random();
		return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
	}


	private Error? ValidateCustomerUniqueness(Customer? existedCustomer, Customer newCustomer)
	{
		if (existedCustomer is null)
			return null;
		if (existedCustomer.Email == newCustomer.Email &&
			existedCustomer.PhoneNumber.Number == newCustomer.PhoneNumber.Number)
			return CustomerErrors.EmailAndPhoneAlreadyExists;

		if (existedCustomer.Email == newCustomer.Email)
			return CustomerErrors.EmailAlreadyExists;

		if (existedCustomer.PhoneNumber.Number == newCustomer.PhoneNumber.Number)
			return CustomerErrors.PhoneAlreadyExists;


		return null;
	}

	private string GetShortHash(string email)
	{
		email = email.ToLower().Trim();

		using (SHA256 sha256 = SHA256.Create())
		{
			byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));


			string base64 = Convert.ToBase64String(hash)
				.Replace("+", "-")
				.Replace("/", "_")
				.Replace("=", "");

			return base64;
		}
	}
}
