using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Tokens;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database;

public class UsersDbContext : IdentityDbContext<User, Role, string,
	UserClaim, UserRole, UserLogin,
	RoleClaim, UserToken>
{
	public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

	public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.HasDefaultSchema(DbConsts.UsersSchema);

		// Only apply configurations that exist in the OrganizationAggregate namespace/folder
		modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(OrganizationsDbContext).Assembly,
				type => type.Namespace != null && type.Namespace.Contains("UsersAggregate"));
	}
}
