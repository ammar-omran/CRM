using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class Role : IdentityRole
{
	public string? PairentRoleId { get; set; } = default!;
	public Role? PairentRole { get; set; } = default!;
	public ICollection<Role> ChildRoles { get; set; } = [];

	public ICollection<RoleClaim> RoleClaims { get; set; } = null!;
}
