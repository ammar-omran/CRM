using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.UsersAggregate;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		builder.ToTable("Roles");

		// Self-referencing parent/child
		builder.HasOne(r => r.PairentRole)
				.WithMany(r => r.ChildRoles)
				.HasForeignKey(r => r.PairentRoleId)
				.OnDelete(DeleteBehavior.Restrict);

		// Each Role can have many associated RoleClaims
		builder.HasMany(e => e.RoleClaims)
				.WithOne(e => e.Role)
				.HasForeignKey(rc => rc.RoleId)
				.IsRequired();

		builder.HasData([
				new Role { Id = "1", Name = "Agent", NormalizedName = "AGENT", ConcurrencyStamp = "f0bd1f07-f514-4317-9657-5bb7db4d57c3" },
				new Role { Id = "2", Name = "Supervisor", NormalizedName = "SUPERVISOR", ConcurrencyStamp =  "8e0f28f1-76ee-4f53-82df-133091ecdb94"},
				new Role { Id = "3", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "0f83831b-220a-48a9-9fce-f9c148ec0b78" },

				new Role { Id = "4", Name = "TeamLead", PairentRoleId = "1", NormalizedName = "TEAMLEAD", ConcurrencyStamp = "bf4d84e9-fb60-458f-b176-d6d9e94e1218" },
				new Role { Id = "5", Name = "FirstLine", PairentRoleId = "1", NormalizedName = "FIRSTLINE", ConcurrencyStamp = "afdb8cfc-85ee-471e-9dbc-4508ba2ef533" },
				new Role { Id = "6", Name = "SecondLine", PairentRoleId = "1", NormalizedName = "SECONDLINE", ConcurrencyStamp = "e647f1a9-a664-4269-8c3d-20401c1dccd5" },
			]);
	}
}
