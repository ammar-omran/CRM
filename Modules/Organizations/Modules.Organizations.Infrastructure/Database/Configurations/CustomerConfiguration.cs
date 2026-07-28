using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database.Configurations;

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
    }
}
