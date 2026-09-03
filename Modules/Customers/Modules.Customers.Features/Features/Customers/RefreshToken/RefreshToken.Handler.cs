using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Authentication;

namespace Modules.Customers.Features.Customers.RefreshToken;

public sealed record RefreshTokenRequest(string Token, string RefreshToken);

public interface IRefreshTokenHandler : IHandler
{
    Task<Result<CustomerRefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}

internal sealed class RefreshTokenHandler(ICustomerAuthorizationService authService) : IRefreshTokenHandler
{
    public Task<Result<CustomerRefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        => authService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
}
