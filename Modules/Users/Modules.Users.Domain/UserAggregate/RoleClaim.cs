using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class RoleClaim : IdentityRoleClaim<string>
{
	public Role Role { get; set; } = null!;
}
