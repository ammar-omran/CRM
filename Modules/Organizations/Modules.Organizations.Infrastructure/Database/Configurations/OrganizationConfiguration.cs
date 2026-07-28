using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        entity.HasIndex(x => x.Name)
            .IsUnique();

        entity.Property(o => o.CreatedAt)
            .IsRequired();

        entity.HasMany(o => o.OrganizationAgents)
            .WithOne(oa => oa.Organization)
            .HasForeignKey(oa => oa.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
