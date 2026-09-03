using Microsoft.AspNetCore.Identity;
using CRM.SharedKernel.Domain.Results;

namespace Modules.Users.Domain.Errors;

public static class UserErrors
{
    private const string ErrorPrefix = "Users";

    public static Error NotFound(string userId) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(NotFound)}", $"User with ID {userId} not found");
    
    public static Error NotFoundByEmail(string email) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(NotFound)}", $"User with email {email} not found");
    
    public static Error RegistrationFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(RegistrationFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));
    
    public static Error UpdateFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(UpdateFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));
    
    public static Error DeleteFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(DeleteFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));
    
    public static Error RefreshFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(RefreshFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));
    
    public static Error RoleNotFound(string roleName) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(RoleNotFound)}", $"Role '{roleName}' not found");
    
    public static Error UpdateRoleFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(UpdateRoleFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));

    public static Error InvalidCredentials() =>
        Error.Validation($"{ErrorPrefix}.{nameof(InvalidCredentials)}", "Invalid email or password");
    
    public static Error InvalidToken() =>
        Error.Validation($"{ErrorPrefix}.{nameof(InvalidToken)}", "Invalid token");

    public static Error UserNotActive() =>
        Error.Unauthorized($"{ErrorPrefix}.{nameof(UserNotActive)}", "User account is inactive.");

    public static Error UserAlreadyActive(string email) =>
        Error.Conflict($"{ErrorPrefix}.{nameof(UserAlreadyActive)}", $"User '{email}' already has an active account.");

    public static Error WeakPassword(string details) =>
        Error.Validation($"{ErrorPrefix}.{nameof(WeakPassword)}", $"Password does not meet requirements: {details}");

    public static Error InvalidOrExpiredLink() =>
        Error.Validation($"{ErrorPrefix}.{nameof(InvalidOrExpiredLink)}", "Invalid or expired link.");

    public static Error TokenExpired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(TokenExpired)}", "This link has expired. Please contact your administrator.");

    public static Error LockedOut(DateTimeOffset? lockoutEnd) =>
        Error.Unauthorized($"{ErrorPrefix}.{nameof(LockedOut)}", lockoutEnd.HasValue
            ? $"Account is locked out until {lockoutEnd.Value:O}."
            : "Account is locked out.");
}
