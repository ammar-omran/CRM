using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.OrganizationAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.OrganizationAggregate;

public class OrganizationAgentConfiguration : IEntityTypeConfiguration<OrganizationAgent>
{
    public void Configure(EntityTypeBuilder<OrganizationAgent> builder)
    {
        builder.HasKey(oa => oa.Id);

        builder.HasOne(oa => oa.Organization)
            .WithMany(o => o.OrganizationAgents)
            .HasForeignKey(oa => oa.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oa => oa.Agent)
            .WithMany(a => a.AgentOrganizations)
            .HasForeignKey(oa => oa.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oa => oa.AgentRole)
            .WithMany()
            .HasForeignKey(oa => oa.AgentRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
