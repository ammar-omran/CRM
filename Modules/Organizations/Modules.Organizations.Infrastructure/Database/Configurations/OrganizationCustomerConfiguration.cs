using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database.Configurations;

public class OrganizationCustomerConfiguration : IEntityTypeConfiguration<OrganizationCustomer>
{
    public void Configure(EntityTypeBuilder<OrganizationCustomer> builder)
    {
        builder.HasKey(oc => oc.Id);

        builder.HasIndex(oc => new { oc.OrganizationId, oc.CustomerId })
            .IsUnique();

        builder.HasOne(oc => oc.Organization)
            .WithMany(o => o.OrganizationCustomers)
            .HasForeignKey(oc => oc.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oc => oc.Customer)
            .WithMany(c => c.OrganizationCustomers)
            .HasForeignKey(oc => oc.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
