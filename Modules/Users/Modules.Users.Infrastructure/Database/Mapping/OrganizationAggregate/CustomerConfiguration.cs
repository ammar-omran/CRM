using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.OrganizationAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.OrganizationAggregate;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
	public void Configure(EntityTypeBuilder<Customer> entity)
	{
		entity.HasKey(x => x.Id);

		entity.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(128);

		entity.Property(x => x.Email)
				.IsRequired()
				.HasMaxLength(256);

		entity.Property(x => x.ReferenceId)
				.IsRequired();
		entity.HasIndex(x => x.ReferenceId);

		entity.Property(x => x.CreatedAt)
				.IsRequired();

		entity.HasOne(c => c.Organization)
				.WithMany(o => o.OrganizationCustomers)
				.HasForeignKey(c => c.OrganizationId)
				.OnDelete(DeleteBehavior.Cascade);
	}
}
