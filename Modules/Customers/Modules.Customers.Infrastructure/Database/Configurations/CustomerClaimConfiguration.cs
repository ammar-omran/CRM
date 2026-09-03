using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Database.Configurations;

public class CustomerClaimConfiguration : IEntityTypeConfiguration<CustomerClaim>
{
    public void Configure(EntityTypeBuilder<CustomerClaim> builder)
    {
        builder.ToTable("CustomerClaims");
    }
}
