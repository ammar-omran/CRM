using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.UserAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.OrganizationAggregate;

/// <summary>
/// Maps <see cref=\"User\"/> as a read-only reference in <see cref=\"Database.OrganizationsDbContext\"/>.
/// The table lives in the <c>users</c> schema and is owned/migrated by <see cref=\"Database.UsersDbContext\"/>.
/// This allows <c>Agent.UserId -> User.Id</c> FK without creating a duplicate table in the <c>org</c> schema,
/// following the same pattern as <see cref=\"AgentRoleConfiguration\"/> for <c>OrganizationAgent.AgentRoleId -> Role.Id</c>.
/// </summary>
public class UserReferenceConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users", DbConsts.UsersSchema, t => t.ExcludeFromMigrations());
		builder.HasKey(u => u.Id);
		builder.Property(u => u.Id).HasColumnName("Id").HasMaxLength(450);
		builder.Property(u => u.Email).HasColumnName("Email").HasMaxLength(256);
		builder.Property(u => u.UserName).HasColumnName("UserName").HasMaxLength(256);
		builder.Property(u => u.NormalizedEmail).HasColumnName("NormalizedEmail").HasMaxLength(256);
		builder.Property(u => u.NormalizedUserName).HasColumnName("NormalizedUserName").HasMaxLength(256);
		// Ignore navigation / domain properties that are not needed for the FK reference
		builder.Ignore(u => u.Claims);
		builder.Ignore(u => u.UserRoles);
		builder.Ignore(u => u.UserLogins);
		builder.Ignore(u => u.UserTokens);
		builder.Ignore(u => u.IsActive);
		builder.Ignore(u => u.CreatedAt);
		builder.Ignore(u => u.UpdatedAt);
		builder.Ignore(u => u.ConcurrencyStamp);
		builder.Ignore(u => u.SecurityStamp);
		builder.Ignore(u => u.PasswordHash);
		builder.Ignore(u => u.PhoneNumber);
		builder.Ignore(u => u.PhoneNumberConfirmed);
		builder.Ignore(u => u.TwoFactorEnabled);
		builder.Ignore(u => u.LockoutEnd);
		builder.Ignore(u => u.LockoutEnabled);
		builder.Ignore(u => u.AccessFailedCount);
		builder.Ignore(u => u.EmailConfirmed);
	}
}
