using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Authentication;

namespace Modules.Customers.Features.Customers.Register;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string CountryCode,
    string Password
);

public sealed record RegisterResponse(string HashedEmail, string MaskedEmail);

public interface IRegisterHandler : IHandler
{
    Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken);
}

internal sealed class RegisterHandler(ICustomerAuthorizationService authService) : IRegisterHandler
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.Name, request.Email, request.PhoneNumber, request.CountryCode, request.Password, cancellationToken);
        if (result.IsError)
            return result.Errors;

        return new RegisterResponse(result.Value!.HashedEmail, result.Value!.MaskedEmail);
    }
}
