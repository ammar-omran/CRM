using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class UserClaim : IdentityUserClaim<string>
{
	public User User { get; set; } = null!;
}
