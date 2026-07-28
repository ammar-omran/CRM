using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Authentication;

namespace Modules.Users.Features.Users.UpdateUserRole;

internal interface IUpdateUserRoleHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateUserRoleHandler(
    IClientAuthorizationService authorizationService)
    : IUpdateUserRoleHandler
{
    public async Task<Result<Success>> HandleAsync(
        string userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authorizationService.UpdateUserRoleAsync(userId, request.NewRole, cancellationToken);
        return result;
    }
}
