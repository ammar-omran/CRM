using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.OrganizationAggregate;

public class AgentRoleConfiguration : IEntityTypeConfiguration<Role>
{
	// Role lives in the users schema and is owned/migrated by UsersDbContext.
	// Map it read-only into this model so OrganizationAgent.AgentRole can be
	// navigated/included, while migrations never touch the shared table.
	// Column names are explicit because this context does not use snake-case naming.
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		builder.ToTable("Roles", DbConsts.UsersSchema, t => t.ExcludeFromMigrations());
		builder.HasKey(r => r.Id);
		builder.Property(r => r.Id).HasColumnName("id").HasMaxLength(450);
		builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(256);
		builder.Property(r => r.NormalizedName).HasColumnName("normalized_name").HasMaxLength(256);
		builder.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");
		builder.Ignore(r => r.PairentRoleId);
		builder.Ignore(r => r.PairentRole);
		builder.Ignore(r => r.RoleClaims);
	}
}
