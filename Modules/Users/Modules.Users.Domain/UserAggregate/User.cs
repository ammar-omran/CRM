using Microsoft.AspNetCore.Identity;
using CRM.SharedKernel.Domain;

namespace Modules.Users.Domain.UserAggregate;

public class User : IdentityUser, IAuditableEntity
{
    public ICollection<UserClaim> Claims { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = null!;

    public ICollection<UserLogin> UserLogins { get; set; } = null!;

    public ICollection<UserToken> UserTokens { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the account is active and allowed to authenticate.
    /// New accounts are inactive by default (pending first-time password setup)
    /// and cannot log in until they complete SetPassword to activate the account.
    /// </summary>
    public bool IsActive { get; set; } = false;
}
