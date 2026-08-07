using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.EntitiesConfiguration
{
    public class SeverityConfiguration : IEntityTypeConfiguration<Severity>
    {
        public void Configure(EntityTypeBuilder<Severity> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name).HasMaxLength(100).IsRequired();

            builder.HasData(
               new Severity { Id = 1, Name = "Low", IsVisible = true, Sort = 1 },
               new Severity { Id = 2, Name = "Medium", IsVisible = true, Sort = 2 },
               new Severity { Id = 3, Name = "High", IsVisible = true, Sort = 3 },
               new Severity { Id = 4, Name = "Critical", IsVisible = true, Sort = 4 }
            );
        }
    }
}
