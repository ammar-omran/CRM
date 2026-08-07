using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class Role : IdentityRole
{
	public ICollection<UserRole> UserRoles { get; set; } = null!;
	public ICollection<RoleClaim> RoleClaims { get; set; } = null!;
}
