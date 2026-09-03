using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Authentication;

namespace Modules.Customers.Features.Customers.ConfirmEmail;

public sealed record ConfirmEmailRequest(string HashedEmail, string Otp);

public interface IConfirmEmailHandler : IHandler
{
    Task<Result<Success>> HandleAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
}

internal sealed class ConfirmEmailHandler(ICustomerAuthorizationService authService) : IConfirmEmailHandler
{
    public Task<Result<Success>> HandleAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
        => authService.ConfirmEmailAsync(request.HashedEmail, request.Otp, cancellationToken);
}
