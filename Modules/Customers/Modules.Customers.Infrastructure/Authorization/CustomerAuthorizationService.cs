using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Configuration;
using CRM.SharedKernel.Infrastructure.Services;
using Modules.Customers.Domain.Authentication;
using Modules.Customers.Domain.Entities;
using Modules.Customers.Domain.Errors;
using Modules.Customers.Domain.Tokens;
using Modules.Customers.Infrastructure.Database;

namespace Modules.Customers.Infrastructure.Authorization;

public class CustomerAuthorizationService(
    UserManager<Customer> userManager,
    SignInManager<Customer> signInManager,
    RoleManager<CustomerRole> roleManager,
    ILogger<CustomerAuthorizationService> logger,
    IOptions<AuthConfiguration> authOptions,
    TokenValidationParameters tokenValidationParameters,
    CustomersDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration)
    : ICustomerAuthorizationService
{
    public async Task<Result<RegisterCustomerResponse>> RegisterAsync(string name, string email, string phoneNumber, string countryCode, string password, CancellationToken cancellationToken)
    {
        // Check uniqueness via Identity (email) and phone
        var existingByEmail = await userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
            return CustomerErrors.EmailAlreadyExists;

        var phoneDigits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        var existingByPhone = await dbContext.Set<Customer>()
            .FirstOrDefaultAsync(c => c.PhoneNumber.Number == phoneDigits, cancellationToken);
        if (existingByPhone is not null)
            return CustomerErrors.PhoneAlreadyExists;

        // Also check both together for specific message
        if (existingByEmail is not null && existingByPhone is not null)
            return CustomerErrors.EmailAndPhoneAlreadyExists;

        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Email = email,
            UserName = name,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = name.ToUpperInvariant(),
            PhoneNumber = new PhoneNumber(phoneDigits, countryCode.StartsWith("+") ? countryCode : "+" + countryCode),
            IsEmailVerified = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(customer, password);
        if (!createResult.Succeeded)
        {
            logger.LogWarning("Failed to register customer {Email}: {@Errors}", email, createResult.Errors);
            return CustomerErrors.RegistrationFailed(createResult.Errors);
        }

        // Ensure default role Customer exists and assign
        const string defaultRole = "Customer";
        if (!await roleManager.RoleExistsAsync(defaultRole))
        {
            var role = new CustomerRole { Id = Guid.NewGuid().ToString(), Name = defaultRole, NormalizedName = defaultRole.ToUpperInvariant() };
            await roleManager.CreateAsync(role);
        }
        // Do not assign role until email verified? Users assigns immediately, but spec says default role Customer - we assign after verification or now?
        // Follow Users pattern: assign after activation. Keep inactive without role, assign on confirm.
        // But to follow spec, we will assign on confirm, not here.

        var otp = GenerateOtp();
        var hashedEmail = GetShortHash(email);
        var maskedEmail = Regex.Replace(email, @"(^.).*(?=@)", m => m.Value.Length > 2 ? $"{m.Value[0]}***{m.Value[^1]}" : $"{m.Value[0]}***");

        customer.OTP = otp;
        customer.HashedEmail = hashedEmail;
        customer.OTPCreatedDate = DateTime.UtcNow;
        customer.FailedAttempts = 0;
        customer.IsLocked = false;

        var updateResult = await userManager.UpdateAsync(customer);
        if (!updateResult.Succeeded)
        {
            logger.LogWarning("Failed to set OTP for customer {Email}: {@Errors}", email, updateResult.Errors);
            return CustomerErrors.RegistrationFailed(updateResult.Errors);
        }

        var otpExpiration = configuration.GetValue<int>("OTPEXPIRATION", 5);
        try
        {
            await emailSender.SendAsync(customer.Email!,
                $"Welcome to Our Service, OTP: {otp}",
                $"Thank you for registering with us!, your OTP: {otp}, please note that the otp will expire after {otpExpiration} minute(s)",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send OTP email to {Email}, but registration succeeded", email);
            // Return success even if email fails, as in old service
        }

        logger.LogInformation("Customer registered: {Email} with HashedEmail {HashedEmail}", email, hashedEmail);

        return new RegisterCustomerResponse(hashedEmail, maskedEmail);
    }

    public async Task<Result<Success>> ConfirmEmailAsync(string hashedEmail, string otp, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Set<Customer>().FirstOrDefaultAsync(c => c.HashedEmail == hashedEmail, cancellationToken);
        if (customer is null)
            return CustomerErrors.OtpInvalid;

        if (customer.IsEmailVerified)
            return CustomerErrors.AlreadyVerified;

        if (customer.IsLocked)
            return CustomerErrors.OtpLocked;

        var otpExpiration = configuration.GetValue<int>("OTPEXPIRATION", 5);
        if (customer.OTPCreatedDate.AddMinutes(otpExpiration) < DateTime.UtcNow)
            return CustomerErrors.OtpExpired;

        if (customer.OTP != otp)
        {
            customer.FailedAttempts++;
            if (customer.FailedAttempts >= 3)
            {
                customer.IsLocked = true;
                await userManager.UpdateAsync(customer);
                return CustomerErrors.OtpLocked;
            }
            await userManager.UpdateAsync(customer);
            return CustomerErrors.OtpInvalid;
        }

        // Success - activate
        customer.IsEmailVerified = true;
        customer.IsActive = true;
        customer.IsLocked = false;
        customer.FailedAttempts = 0;
        customer.OTP = string.Empty;
        // Keep HashedEmail for reference? Clear OTP only
        customer.EmailConfirmed = true;

        var updateResult = await userManager.UpdateAsync(customer);
        if (!updateResult.Succeeded)
            return CustomerErrors.RegistrationFailed(updateResult.Errors);

        // Assign default role Customer
        const string defaultRole = "Customer";
        if (!await roleManager.RoleExistsAsync(defaultRole))
        {
            var role = new CustomerRole { Id = Guid.NewGuid().ToString(), Name = defaultRole, NormalizedName = defaultRole.ToUpperInvariant() };
            await roleManager.CreateAsync(role);
        }
        if (!await userManager.IsInRoleAsync(customer, defaultRole))
        {
            var addRoleResult = await userManager.AddToRoleAsync(customer, defaultRole);
            if (!addRoleResult.Succeeded)
                return CustomerErrors.RegistrationFailed(addRoleResult.Errors);
        }

        logger.LogInformation("Customer email verified: {HashedEmail}", hashedEmail);
        return Result.Success;
    }

    public async Task<Result<Success>> ResendOtpAsync(string hashedEmail, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Set<Customer>().FirstOrDefaultAsync(c => c.HashedEmail == hashedEmail, cancellationToken);
        if (customer is null)
            return Error.NotFound("Customer.NotFound", "Customer not found.");

        if (customer.IsEmailVerified)
            return CustomerErrors.AlreadyVerified;

        var resendCooldown = configuration.GetValue<int>("ResendCooldown", 3);
        if (customer.OTPCreatedDate.AddMinutes(resendCooldown) > DateTime.UtcNow)
            return CustomerErrors.ResendCooldown;

        var otp = GenerateOtp();
        customer.OTP = otp;
        customer.OTPCreatedDate = DateTime.UtcNow;
        customer.FailedAttempts = 0;
        customer.IsLocked = false;

        var updateResult = await userManager.UpdateAsync(customer);
        if (!updateResult.Succeeded)
            return CustomerErrors.RegistrationFailed(updateResult.Errors);

        var otpExpiration = configuration.GetValue<int>("OTPEXPIRATION", 5);
        try
        {
            await emailSender.SendAsync(customer.Email!,
                "Resend Verification Code",
                $"<p>Your new verification code:</p><p>{otp}</p><p>This code is valid for {otpExpiration} minutes only.</p>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resend OTP to {Email}", customer.Email);
            return Error.Failure("Customer.EmailFailed", "Failed to send OTP email. Please try again later.");
        }

        return Result.Success;
    }

    public async Task<Result<CustomerLoginResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var customer = await userManager.FindByEmailAsync(email);
        if (customer is null)
            return CustomerErrors.NotFoundByEmail(email);

        if (!customer.IsActive || !customer.IsEmailVerified)
            return CustomerErrors.CustomerNotActive;

        var result = await signInManager.CheckPasswordSignInAsync(customer, password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            logger.LogWarning("Customer {Email} is locked out until {LockoutEnd}", email, customer.LockoutEnd);
            return CustomerErrors.LockedOut(customer.LockoutEnd);
        }

        if (!result.Succeeded)
            return CustomerErrors.InvalidCredentials;

        var (token, refreshToken) = await GenerateJwtAndRefreshTokenAsync(customer, null);
        return new CustomerLoginResponse(token, refreshToken);
    }

    public async Task<Result<CustomerRefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
    {
        var validatedToken = GetPrincipalFromToken(token, tokenValidationParameters);
        if (validatedToken is null)
            return CustomerErrors.InvalidToken;

        var jti = validatedToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
            return CustomerErrors.InvalidToken;

        var storedRefreshToken = await dbContext.Set<CustomerRefreshToken>().FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
        if (storedRefreshToken is null)
        {
            logger.LogWarning("Refresh token does not exist");
            return CustomerErrors.InvalidToken;
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            logger.LogWarning("Refresh token has expired");
            return CustomerErrors.InvalidToken;
        }

        if (storedRefreshToken.Invalidated)
        {
            logger.LogWarning("Refresh token has been invalidated");
            return CustomerErrors.InvalidToken;
        }

        if (storedRefreshToken.JwtId != jti)
        {
            logger.LogWarning("Refresh token does not match JWT");
            return CustomerErrors.InvalidToken;
        }

        var customerId = validatedToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value
            ?? validatedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value
            ?? validatedToken.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;

        if (customerId is null)
            return CustomerErrors.InvalidToken;

        var customer = await userManager.FindByIdAsync(customerId);
        if (customer is null)
            return CustomerErrors.NotFound(customerId);

        var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(customer, refreshToken);
        return new CustomerRefreshTokenResponse(newToken, newRefreshToken);
    }

    private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(Customer customer, string? existingRefreshToken)
    {
        var roleNames = await userManager.GetRolesAsync(customer);
        if (roleNames.Count == 0)
            roleNames = new List<string> { "Customer" };

        var allClaims = new List<Claim>();
        var seenClaims = new HashSet<string>(StringComparer.Ordinal);
        var visitedRoleIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var roleName in roleNames)
        {
            var current = await roleManager.FindByNameAsync(roleName);
            while (current is not null && visitedRoleIds.Add(current.Id))
            {
                var claims = await roleManager.GetClaimsAsync(current);
                foreach (var claim in claims)
                {
                    var key = $"{claim.Type}{(char)0x1F}{claim.Value}";
                    if (seenClaims.Add(key))
                        allClaims.Add(claim);
                }
                // Customer roles have no hierarchy, but keep for consistency
                current = null;
            }
        }

        var userClaims = await userManager.GetClaimsAsync(customer);
        foreach (var claim in userClaims)
        {
            var key = $"{claim.Type}{(char)0x1F}{claim.Value}";
            if (seenClaims.Add(key))
                allClaims.Add(claim);
        }

        var token = GenerateJwtToken(customer, authOptions.Value, roleNames, allClaims);
        var refreshToken = await GenerateRefreshTokenAsync(token, customer, existingRefreshToken);
        return (token, refreshToken);
    }

    private async Task<string> GenerateRefreshTokenAsync(string token, Customer customer, string? existingRefreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var jti = jwtToken.Id;

        var refreshToken = new CustomerRefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = jti,
            CustomerId = customer.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
        };

        if (!string.IsNullOrEmpty(existingRefreshToken))
        {
            var existingToken = await dbContext.Set<CustomerRefreshToken>().FirstOrDefaultAsync(x => x.Token == existingRefreshToken);
            if (existingToken != null)
                dbContext.Set<CustomerRefreshToken>().Remove(existingToken);
        }

        await dbContext.Set<CustomerRefreshToken>().AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    private static string GenerateJwtToken(Customer customer, AuthConfiguration authConfiguration, IEnumerable<string> roles, IList<Claim> roleClaims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenId = Guid.NewGuid().ToString();
        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, customer.Id),
            new(JwtRegisteredClaimNames.Sub, customer.Id),
            new(ClaimTypes.Name, customer.UserName ?? customer.Name ?? string.Empty),
            new(ClaimTypes.Email, customer.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, tokenId)
        ];

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var roleClaim in roleClaims)
            claims.Add(new Claim(roleClaim.Type, roleClaim.Value));

        var token = new JwtSecurityToken(
            issuer: authConfiguration.Issuer,
            audience: authConfiguration.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static ClaimsPrincipal? GetPrincipalFromToken(string token, TokenValidationParameters parameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var tokenValidationParameters = parameters.Clone();
#pragma warning disable CA5404
            tokenValidationParameters.ValidateLifetime = false;
#pragma warning restore CA5404
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch { return null; }
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        => validatedToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

    private static string GenerateOtp(int length = 6)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
    }

    private static string GetShortHash(string email)
    {
        email = email.ToLowerInvariant().Trim();
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
        return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
