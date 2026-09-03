using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Features.Users.DeleteUser;

public interface IDeleteUserHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, CancellationToken cancellationToken);
}

internal sealed class DeleteUserHandler(
    UserManager<User> userManager,
    ILogger<DeleteUserHandler> logger)
    : IDeleteUserHandler
{
    public async Task<Result<Success>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogInformation("User with ID {UserId} not found", userId);
            return UserErrors.NotFound(userId);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to delete user: {@Errors}", result.Errors);
            return UserErrors.DeleteFailed(result.Errors);
        }

        logger.LogInformation("Deleted user with ID: {UserId}", userId);
        return Result.Success;
    }
}
