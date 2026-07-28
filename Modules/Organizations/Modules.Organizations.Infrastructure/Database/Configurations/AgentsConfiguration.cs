using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Infrastructure.Database.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();

        builder.HasMany(a => a.AgentOrganizations)
            .WithOne(oa => oa.Agent)
            .HasForeignKey(oa => oa.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
