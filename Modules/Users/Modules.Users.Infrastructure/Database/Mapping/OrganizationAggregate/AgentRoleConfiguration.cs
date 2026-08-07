using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.OrganizationAggregate;

namespace Modules.Users.Infrastructure.Database.Mapping.OrganizationAggregate;

public class AgentRoleConfiguration : IEntityTypeConfiguration<AgentRole>
{
    public void Configure(EntityTypeBuilder<AgentRole> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(64);
        
        
        entity.HasIndex(x => x.Name)
            .IsUnique();

        entity.HasData(
            new AgentRole
            {
                Id = 1,
                Name = "FirstLine",
            },
            new AgentRole
            {
                Id = 2,
                Name = "SecondLine",
            },
            new AgentRole
            {
                Id = 3,
                Name = "TeamLead",
            }
        );
    }
}
