using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Database.Configurations;

public class CustomerUserRoleConfiguration : IEntityTypeConfiguration<CustomerUserRole>
{
    public void Configure(EntityTypeBuilder<CustomerUserRole> builder)
    {
        builder.ToTable("CustomerUserRoles");
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Customer)
            .WithMany(c => c.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
