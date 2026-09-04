using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.UserAggregate;
using Modules.Users.Features.Users.Shared;

namespace Modules.Users.Features.Users.RegisterUser;

public interface IRegisterUserHandler : IHandler
{
    Task<Result<UserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}

internal sealed class RegisterUserHandler(
    UserManager<User> userManager,
    ILogger<RegisterUserHandler> logger)
    : IRegisterUserHandler
{
    public async Task<Result<UserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Name,
            PhoneNumber = request.Phone,
        };

        IdentityResult result;
        if (!string.IsNullOrWhiteSpace(request.Password))
            result = await userManager.CreateAsync(user, request.Password);
        else
            result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogInformation("Failed to register user: {@Errors}", result.Errors);
            return UserErrors.RegistrationFailed(result.Errors);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            await userManager.AddToRoleAsync(user, request.Role);
        }

        logger.LogInformation("Created user with ID: {UserId}", user.Id);

        return new UserResponse(user.Id, user.Email);
    }
}
