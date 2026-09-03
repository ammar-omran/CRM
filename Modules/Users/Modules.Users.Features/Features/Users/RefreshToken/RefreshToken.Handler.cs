using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Authentication;

namespace Modules.Users.Features.Users.RefreshToken;

public interface IRefreshTokenHandler : IHandler
{
    Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}

internal sealed class RefreshTokenHandler(IClientAuthorizationService authorizationService)
    : IRefreshTokenHandler
{
    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authorizationService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result;
    }
}
