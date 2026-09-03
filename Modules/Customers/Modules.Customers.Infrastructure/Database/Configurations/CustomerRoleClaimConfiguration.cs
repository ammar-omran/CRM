using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Database.Configurations;

public class CustomerRoleClaimConfiguration : IEntityTypeConfiguration<CustomerRoleClaim>
{
    public void Configure(EntityTypeBuilder<CustomerRoleClaim> builder)
    {
        builder.ToTable("CustomerRoleClaims");
    }
}
