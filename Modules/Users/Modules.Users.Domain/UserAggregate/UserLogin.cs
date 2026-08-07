using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.UserAggregate;

public class UserLogin : IdentityUserLogin<string>
{
	public User User { get; set; } = null!;
}
