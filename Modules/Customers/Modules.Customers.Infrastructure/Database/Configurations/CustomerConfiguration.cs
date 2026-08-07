using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.OrganizationAggregate.

namespace Modules.Customers.Infrastructure.Database.Mapping.OrganizationAggregate;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Password)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.OTP)
            .HasMaxLength(10);

        entity.Property(x => x.HashedEmail)
            .HasMaxLength(200);

        entity.HasIndex(x => x.Email)
            .IsUnique();

        entity.Ignore(x => x.PhoneNumber);
    }
}
