using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Errors;
using Modules.Customers.Infrastructure.Database;
using CRM.SharedKernel.Infrastructure.Services;
using System.Security.Claims;

namespace Modules.Customers.Features.Customers.Login;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(
	bool success,
	string message,
	string? token = null,
	int? customerId = null,
	string? customerName = null,
	string? customerEmail = null);

internal interface ILoginHandler : IHandler
{
	Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken);
}

internal sealed class LoginHandler(
	CustomersDbContext context,
	IPasswordHasher passwordHasher,
	IJwtTokenGenerator jwtTokenGenerator,
	ILogger<LoginHandler> logger)
	: ILoginHandler
{
	public async Task<Result<LoginResponse>> HandleAsync(
		LoginRequest request,
		CancellationToken cancellationToken)
	{
		var customer = await context.Customers
			.FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

		if (customer is null)
		{
			logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
			return CustomerErrors.InvalidCredentials;
		}

		if (customer.IsLocked)
		{
			logger.LogWarning("Login attempt for locked account: {Email}", request.Email);
			return CustomerErrors.OtpLocked;
		}

		if (!customer.IsEmailVerified)
		{
			logger.LogWarning("Login attempt for unverified email: {Email}", request.Email);
			return CustomerErrors.EmailNotVerified;
		}

		bool isValidPassword = passwordHasher.Verify(request.Password, customer.Password);
		if (!isValidPassword)
			return CustomerErrors.InvalidCredentials;

		logger.LogInformation("Customer logged in: {Email}", request.Email);

		string token = jwtTokenGenerator.GenerateToken(customer.Id.ToString(), customer.Email, [new Claim(ClaimTypes.Name, customer.Name)]);

		// Send login announcement email immediately after successful login.
		// A transient email failure must never deny access; log it and continue.
		// await SendLoginNotificationAsync(
		// 	customerEmail: customer.Email,
		// 	customerName: customer.Name,
		// 	deviceInfo: request.DeviceInfo,
		// 	cancellationToken: cancellationToken);

		return new LoginResponse(true, "Login successful.", token, customer.Id, customer.Name, customer.Email);
	}
}
