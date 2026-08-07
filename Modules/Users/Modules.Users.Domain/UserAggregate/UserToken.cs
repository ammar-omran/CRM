using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class UserToken : IdentityUserToken<string>
{
	public User User { get; set; } = null!;
}
