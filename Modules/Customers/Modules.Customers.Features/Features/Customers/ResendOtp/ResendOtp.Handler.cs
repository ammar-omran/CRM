using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Authentication;

namespace Modules.Customers.Features.Customers.ResendOtp;

public sealed record ResendOtpRequest(string HashedEmail);

public interface IResendOtpHandler : IHandler
{
    Task<Result<Success>> HandleAsync(ResendOtpRequest request, CancellationToken cancellationToken);
}

internal sealed class ResendOtpHandler(ICustomerAuthorizationService authService) : IResendOtpHandler
{
    public Task<Result<Success>> HandleAsync(ResendOtpRequest request, CancellationToken cancellationToken)
        => authService.ResendOtpAsync(request.HashedEmail, cancellationToken);
}
