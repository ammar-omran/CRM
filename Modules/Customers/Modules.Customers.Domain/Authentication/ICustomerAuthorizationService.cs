using CRM.SharedKernel.Domain.Results;

namespace Modules.Customers.Domain.Authentication;

public interface ICustomerAuthorizationService
{
    Task<Result<CustomerLoginResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<CustomerRefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
    Task<Result<RegisterCustomerResponse>> RegisterAsync(string name, string email, string phoneNumber, string countryCode, string password, CancellationToken cancellationToken);
    Task<Result<Success>> ConfirmEmailAsync(string hashedEmail, string otp, CancellationToken cancellationToken);
    Task<Result<Success>> ResendOtpAsync(string hashedEmail, CancellationToken cancellationToken);
}

public sealed record RegisterCustomerResponse(string HashedEmail, string MaskedEmail);
