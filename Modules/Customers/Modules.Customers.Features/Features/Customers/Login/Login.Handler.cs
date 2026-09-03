using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Authentication;

namespace Modules.Customers.Features.Customers.Login;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, string RefreshToken);

public interface ILoginHandler : IHandler
{
    Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken);
}

internal sealed class LoginHandler(ICustomerAuthorizationService authService) : ILoginHandler
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result.IsError)
            return result.Errors;

        return new LoginResponse(result.Value!.Token, result.Value!.RefreshToken);
    }
}
