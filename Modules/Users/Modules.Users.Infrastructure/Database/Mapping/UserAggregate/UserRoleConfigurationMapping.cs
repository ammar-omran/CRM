using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.UsersAggregate;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
	public void Configure(EntityTypeBuilder<UserRole> builder)
	{
		builder.HasKey(x => new { x.UserId, x.RoleId });

		builder.HasOne(ur => ur.Role)
			.WithMany()
			.HasForeignKey(ur => ur.RoleId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(ur => ur.User)
			.WithMany(u => u.UserRoles)
			.HasForeignKey(ur => ur.UserId)
			.OnDelete(DeleteBehavior.Restrict);

	}
}
