using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Database.Mapping.Entities;

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

        // Map PhoneNumber 
        entity.OwnsOne(x => x.PhoneNumber, pn =>
        {
            pn.Property(p => p.Number)
                .HasColumnName("PhoneNumber_Number")
                .HasMaxLength(20);

            pn.Property(p => p.CountryCode)
                .HasColumnName("PhoneNumber_CountryCode")
                .HasMaxLength(5);

            pn.HasIndex(p => p.Number).HasDatabaseName("IX_Customers_PhoneNumber_Number");
        });
    }
}
